use actix_web::web::{Bytes, Json};
use reqwest::{Client, StatusCode};
use rsa::pkcs8::der::Encodable;
use std::time::Duration;
use base64::{encode};

use crate::instance::config::{RemoteServerReq, REMOTEIP, REMOTE_SERVER};

pub async fn get_drone_image() -> String {
    let client = Client::builder()
        .timeout(Duration::from_millis(500))
        .build()
        .unwrap();
    let remote_ip = REMOTEIP.lock().unwrap().clone();
    let url = format!("{}/drone/video/{}", REMOTE_SERVER, &remote_ip);

    match client.get(&url).send().await {
        Ok(response) => {
            if response.status() == StatusCode::OK {
                let content = response.text().await.expect("RESPONSE ERROR");
                content
            }

            else if response.status() == StatusCode::ACCEPTED {
                let post_url = format!("{}/drone/control/", REMOTE_SERVER);
                let body = RemoteServerReq::new(remote_ip, "stramon".to_string(), "".to_string());
                match client.post(post_url).json(&body).send().await {
                    Ok(response) => {
                        match client.get(&url).send().await {
                            Ok(response) => {
                                if response.status() == StatusCode::OK {
                                    let content = response.text().await.expect("RESPONSE ERROR");
                                    content
                                    // Ok(encode(content))
                                }

                                else {
                                    println!("STREAM NOT RUNNING");
                                    "".to_string()
                                }
                            },
                            Err(_) => {
                                println!("Server Not RUNNING");
                                "".to_string()
                            },
                        }
                    },
                    Err(_) => todo!(),
                }
            }

            else {
                println!("REPONSE ERROR");
                "".to_string()
            }
        },
        Err(e) => {
            println!("REQUEST ERROR");
            "".to_string()
        },
    }
}

pub async fn get_drone_loc() -> Result<String, &'static str> {
    let client = Client::builder()
        .timeout(Duration::from_millis(500))
        .build()
        .unwrap();
    let remote_ip = REMOTEIP.lock().unwrap().clone();
    let url = format!("{}/drone/state/{}", REMOTE_SERVER, remote_ip);

    match client.get(url).send().await {
        Ok(response) => {
            if response.status() == StatusCode::OK {
                let content = response.text().await.expect("RESPONSE ERROR");
                Ok(content)
            }

            else {
                Err("REPONSE ERROR")
            }
        },
        Err(e) => {
            Err("REQUEST ERROR")
        },
    }
}

pub async fn get_car_image() -> Result<String, &'static str> {
    let client = Client::builder()
        .timeout(Duration::from_millis(500))
        .build()
        .unwrap();
    let remote_ip = REMOTEIP.lock().unwrap().clone();
    let url = format!("{}/robot/video/{}", REMOTE_SERVER, &remote_ip);

    match client.get(&url).send().await {
        Ok(response) => {
            if response.status() == StatusCode::OK {
                let content = response.bytes().await.expect("RESPONSE ERROR");
                Ok(encode(content))
            }

            else {
                Err("REPONSE ERROR")
            }
        },
        Err(e) => {
            Err("REQUEST ERROR")
        },
    }
}

pub async fn get_car_loc() -> Result<String, &'static str> {
    let client = Client::builder()
        .timeout(Duration::from_millis(500))
        .build()
        .unwrap();
    let remote_ip = REMOTEIP.lock().unwrap().clone();
    let url = format!("{}/robot/info/{}", REMOTE_SERVER, remote_ip);

    match client.get(url).send().await {
        Ok(response) => {
            if response.status() == StatusCode::OK {
                let content = response.text().await.expect("RESPONSE ERROR");
                Ok(content)
            }

            else {
                Err("REPONSE ERROR")
            }
        },
        Err(e) => {
            Err("REQUEST ERROR")
        },
    }
}