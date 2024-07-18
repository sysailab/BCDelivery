use actix_web::{body, HttpResponse, Responder};
use actix_web::web::Json;
use openssl::conf;
use reqwest::{Client, NoProxy, StatusCode};
use serde::{Serialize, Deserialize};
use serde_json::json;
use tokio::io::Repeat;
use std::collections::HashSet;
use std::fmt::format;
use std::fs::{self, read_link, OpenOptions};
use std::net::Ipv4Addr;
use std::sync::MutexGuard;
use std::time::Duration;
use std::{result, vec};

use local_ip_address;
use tokio;

use crate::instance::config::{self, Node, UpdateNode, GENESIS_NODE, GENESIS_PORT};
use crate::instance::config::{NODES, BLOCKCHAIN, IPADDR, NODE_TYPE};
use crate::{auth, blockchain, get_nodes};

/*
p2p.rs는 블록체인 서버가 행동할때 필요한 함수를 보유하고 있음.
포함되는 함수는, 네트워크 초기설정, 네트워크 모니터링, 합의 요청, 합의 진행, 블록 추가 가 있음.
*/


pub async fn start_monitoring(init_ip: String) {
    // 주기적으로 IP 확인하여 이전 인터넷 환경과 다를 경우 Genesis 노드에게 업데이트 요청
    // 모니터링 주기는 config의 monitoring_time 을 통해 설정됨
    let mut previous_ip = init_ip;
    let mut interval = tokio::time::interval(tokio::time::Duration::from_millis(config::MONITORING_TIME));
    loop {
        interval.tick().await;
        let my_ip = local_ip_address::local_ip().unwrap().to_string();

        if previous_ip != my_ip {
            let client = Client::builder()
                .timeout(Duration::from_millis(1000)) // millisecond
                .build()
                .unwrap();
            let body = UpdateNode::new(previous_ip.clone(), my_ip.clone(), NODE_TYPE.lock().unwrap().clone());
            let url = format!("http:{}:{}/delete-node", config::GENESIS_NODE, config::GENESIS_PORT);

            match client.post(url).json(&body).send().await {
                Ok(response) => {
                    if response.status() == StatusCode::OK {
                        println!("Update success");

                        previous_ip = my_ip.clone();

                        let mut ipaddr = IPADDR.lock().unwrap();
                        *ipaddr = my_ip;                        
                    }
                },

                Err(e) => {
                    println!("REQUEST FAIL : {}", e);
                },
            }
        }
            
    }
}

pub async fn broadcast_nodelist(nodelist: Vec<config::Node>) {
    // 업데이트 된 최신의 Nodelist의 노드들에게 자신이 보유한 nodelist를 전달함
    let client = Client::builder()
        .timeout(Duration::from_millis(1000))
        .build()
        .unwrap(); 
    let body = nodelist.clone();
    for node in nodelist {
        if &node.address != &IPADDR.lock().unwrap().clone() {
            let url = format!("http://{}/broadcast-nodelist", &node.address);
            match client.post(&url).json(&body).send().await {
                Ok(response) => {
                    if response.status() == StatusCode::OK {
                        println!("update success to {}", &node.address)
                    }
                },

                Err(e) => {
                    println!("Update Fail REQUEST ERROR : {}", e)
                },
            }
        }
    }
}

pub async fn send(ip: &str, port: &str) -> Vec<config::Node> {
    // 초기 실행되는 함수
    // IP를 통해 자신이 Genesis 노드인지 확인함
    // 일반 노드의 경우 Genesis 노드에게 자신이 네트워크에 참여함을 알림, 이후 노드 리스트를 반환받음
    let my_ip = ip;
    let my_port = port;
    if my_ip == config::GENESIS_NODE {
        let my_address = format!("{}:{}", my_ip.to_owned(), my_port);
        println!("I am Genesis Node, Network now OPEND!");
        let genesis_node = config::Node::new(my_address, "Genesis".to_owned());
        let mut genesis_vec = Vec::new();
        genesis_vec.push(genesis_node);

        return genesis_vec; // Genesis Node 임을 반환
    }

    else {
        let genesis_ip = config::GENESIS_NODE.to_owned();
        let port = config::GENESIS_PORT;
        let body = config::Node {
            address : format!("{}:{}",&my_ip, &my_port),
            node_type : NODE_TYPE.lock().unwrap().clone()
        };
        let url = format!("http://{}:{}/register-node", &genesis_ip, &port);
        let client = Client::builder()
            .timeout(Duration::from_millis(2000)) // millisecond
            .build()
            .unwrap();

        println!("Register Start : {:?}", &body);

        match client.post(url).json(&body).send().await {
            Ok(response) => {
                if response.status() == StatusCode::OK {
                    match response.json::<Vec<config::Node>>().await {
                        Ok(node_list) => {

                            return node_list;
                        }

                        Err(e) => {
                            println!("REPONSE DATA ERROR {}", e);

                            return  vec![];
                        },
                    }
                }

                else {
                    match response.json::<Vec<config::Node>>().await {
                        Ok(node_list) => {
                            println!("NODE ALREADY EXIST... UPDATE NODE LIST!");

                            return node_list;
                        }
                        
                        Err(e) => {
                            println!("RESPONSE ERROR {}", e);

                            return vec![];
                        }
                    }
                }
            }

            Err(e) => {
                println!("REQUEST ERROR {}", e);

                return vec![];
            }
        }
    }
}

pub async fn vote(block_data: Json<config::BlockData>) -> bool {
    // 키 즉 block_data.sign을 통해 해당 pem키로 서명이 진짜인지 확인함
    auth::check_auth_valid(&block_data.node_type, &block_data.command, &block_data.sign)
}

pub async fn vote_request(block_data: config::BlockData) -> bool {
    // 자신이 보유하고 있는 노드리스트에게 합의 요청을 함.

    // let port = config::GENESIS_PORT;
    let body = &block_data;
    
    let client = Client::builder()
        .timeout(Duration::from_millis(1000)) // millisecond
        .build()
        .unwrap();

    let node_list = NODES.lock().unwrap();
    let mut result_list = Vec::new();
    
    for node in &*node_list {
        let url = format!("http://{}/consensus", &node.address);
        match client.post(&url).json(&body).send().await {
            Ok(response) => {
                if response.status() == StatusCode::OK {
                    match response.json::<config::Result>().await {
                        Ok(response_data) => {
                            let data = config::Vote {
                                addr : node.address.clone(),
                                result : response_data.result,
                            };

                            result_list.push(data)
                        }

                        Err(e) => {
                            println!("RESPONSE DATA ERROR {}", e)
                        },
                    }
                }
            }

            Err(e) => {
                println!("REQUEST ERROR NODE NOT FOUND {}", e)
            },
        }
    }
    
    calculate_vote_result(result_list)
}

pub fn calculate_vote_result(result_list : Vec<config::Vote>) -> bool {
    // 투표 결과를 받아서 전체 결과중 51%가 넘는다면 true반환
    let total_votes = result_list.len();
    let true_count = result_list.iter().filter(|&vote| vote.result).count();

    let percent = (total_votes as f64) * 0.51;
    true_count as f64 > percent
}

pub async fn global_update(block_data: blockchain::Block, ip: String) {
    // 블록이 추가됨에 따라 가지고 있는 노드리스트를 통해 새로운 블록 추가를 요청함
    let my_ip = ip;

    let body = &block_data;

    let client = Client::builder()
        .timeout(Duration::from_millis(500))
        .build()
        .unwrap();
   
    let node_list = NODES.lock().unwrap();

    // For Debug
    println!("Node list : {:?}", &node_list);
    
    for node in &*node_list {
        if &node.address != &my_ip {
            let url = format!("http://{}/broadcast-block", &node.address);
            match client.post(&url).json(&body).send().await {
                Ok(response) => {
                    if response.status() == StatusCode::OK {
                        println!("{} update Success", &node.address)
                    }

                    else {
                        println!("{} update Failed", &node.address)
                    }
                }

                Err(e) => {
                    println!("REQUEST FAIL {}", e)
                }
                
            }
        }
        
    }
}

pub async fn change_remote_mode() -> bool {
    let my_ip = IPADDR.lock().unwrap().clone();
    let my_type = NODE_TYPE.lock().unwrap().clone();

    let node_info = UpdateNode::new(my_ip, "None".to_owned(), my_type);

    let client = Client::builder()
        .timeout(Duration::from_millis(1000))
        .build()
        .unwrap();

    let url = format!("http://{}:{}/delete-node", GENESIS_NODE, GENESIS_PORT);

    match client.post(url).json(&node_info).send().await {
        Ok(response) => {
            // Remote mode start

            let result = true;
            result
        },

        Err(e) => {
            println!("DELETE ERROR : IS GENESIS ALIVE? : {}",e);
            
            let result = false;
            result
        },
    }
}