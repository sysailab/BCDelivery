use actix_web::{web, App, HttpResponse, HttpServer, Responder};
use instance::{config, setup};
use tokio;

mod p2p;

mod blockchain;

mod instance;
use config::{Result, Node, Location, UpdateNode, BlockData};
use config::{BLOCKCHAIN, NODES, IPADDR, PORT};
mod auth;

#[actix_web::main]
async fn main() -> std::io::Result<()> {
    let (my_ip, my_port) = setup::setup_mode().await;    
    
    println!("my addr is {}:{}", &my_ip, &my_port);
    let nodeinfo = p2p::send(&my_ip, &my_port).await;
    {
        let mut nodes_lock = NODES.lock().unwrap();
        *nodes_lock = nodeinfo.clone();
    }
    println!("Nodes info : {:?}", nodeinfo);

    tokio::spawn(p2p::start_monitoring(my_ip.clone()));

    HttpServer::new(|| {
        App::new()
            .route("/register-node", web::post().to(register_node))
            .route("/get-nodes", web::get().to(get_nodes))
            .route("/consensus", web::post().to(consensus))
            .route("/get-last-blockchain", web::get().to(get_last_blockchain))
            .route("/get-all-blockchain", web::get().to(get_all_blockchain))
            .route("/broadcast-block", web::post().to(broadcast_block))
            .route("/add-block", web::post().to(check))
            .route("/broadcast-nodelist", web::post().to(broadcast_nodelist))
            .route("/delete-node", web::post().to(delete_node))
            .route("/get-location", web::get().to(get_location))
            .route("/change-remote-mode", web::post().to(change_remote_mode))
    })
    .bind(format!("0.0.0.0:{}", my_port))?
    .run()
    .await
}

async fn check(req_block_data: web::Json<BlockData>) -> impl Responder {
    let block_data = req_block_data.clone();
    let check_result: bool = p2p::vote_request(block_data).await;
    let response_message = Result::new(check_result);

    // DEBUG
    println!("VOTE Result : {}", &check_result);
    println!{"REQ msg: {:?}", &req_block_data};

    if check_result {
        // DEBUG
        println!("Ready for Broadcast");

        let mut blockchain = BLOCKCHAIN.lock().unwrap();
        
        // DEBUG
        println!("Current Data : {:?}", &blockchain.blocks);

        let my_ip = IPADDR.lock().unwrap().clone();
        let cmd = req_block_data.command.clone();
        println!("ip {}", &my_ip);
        println!("cmd : {}", &cmd);
        let block = blockchain::Data::new(req_block_data.id.clone(), cmd);

        // DEBUG
        println!("Block : {:?}", &block);

        let new_block = blockchain.add_block(block);

        // DEBUG
        println!("block data : {:?}", &new_block);

        p2p::global_update(new_block, my_ip).await;

        HttpResponse::Ok().json(response_message)
    }

    else {
        HttpResponse::Unauthorized().json(response_message)
    }
}

async fn register_node(node_info: web::Json<Node>) -> impl Responder {
    let node = node_info.into_inner();
    let mut nodes = NODES.lock().unwrap();

    if !nodes.iter().any(|n| n.address == node.address) {
        nodes.push(node.clone());
        HttpResponse::Ok().json(&*nodes)
    } else {
        HttpResponse::BadRequest().json(&*nodes)
    }
}

async fn consensus(block_data: web::Json<BlockData>) -> impl Responder {
    let result = p2p::vote(block_data).await;

    let response_message = Result::new(result);
    
    HttpResponse::Ok().json(response_message)
}

async fn get_nodes() -> impl Responder {
    // FUnction For get node list
    let nodes = NODES.lock().unwrap();
    HttpResponse::Ok().json(&*nodes)
}

async fn get_last_blockchain() -> impl Responder {
    // Funciont or get last block of blockchain
    let blockchain = BLOCKCHAIN.lock().unwrap();
    HttpResponse::Ok().json(&*blockchain.get_last_block())
}

async fn get_all_blockchain() -> impl Responder {
    // Get all blockchain datas
    let blockchain = BLOCKCHAIN.lock().unwrap();
    HttpResponse::Ok().json(&*blockchain.blocks)
}

async fn broadcast_block(block_data: web::Json<blockchain::Block>) -> impl Responder {
    // Update Global Blockchain
    let new_block = block_data.into_inner();
    let mut blockchain = BLOCKCHAIN.lock().unwrap();

    blockchain.update_block(new_block);

    println!("New Block Added {:?}", blockchain.get_last_block());

    HttpResponse::Ok().json("Block added")
}

async fn broadcast_nodelist(node_list: web::Json<Vec<Node>>) -> impl Responder {
    let mut pre_nodelist = NODES.lock().unwrap();
    let new_node_list = node_list.into_inner();

    *pre_nodelist = new_node_list;

    HttpResponse::Ok().json("nodelist updated!")
}

async fn delete_node(node_info : web::Json<UpdateNode>) -> impl Responder {
    let node_info = node_info.into_inner();

    let mut node_list = NODES.lock().unwrap();

    node_list.retain(|node| node.address != node_info.delete_ip);

    let new_node = Node::new(node_info.update_ip, node_info.node_type);
    
    node_list.push(new_node);

    p2p::broadcast_nodelist(node_list.clone()).await;

    HttpResponse::Ok().json("Updated!")
}

async fn get_location() -> impl Responder {
    // Location xyz
    let (mut x, mut y, mut z) = ("00.00".to_owned(), "00.00".to_owned(), "00.00".to_owned());

    let reponse = Location::new(x, y, z);
    
    HttpResponse::Ok().json(reponse)
}

async fn change_remote_mode(request : web::Json<BlockData>) -> impl Responder {
    let req_data = request.into_inner();

    let check_result = p2p::vote_request(req_data.clone()).await;
    
    if check_result {
        let mut blockchain = BLOCKCHAIN.lock().unwrap();
        let block = blockchain::Data::new(req_data.id.clone(), req_data.command.clone());
        let new_block = blockchain.add_block(block);

        p2p::global_update(new_block, IPADDR.lock().unwrap().clone()).await;

        let change_result = p2p::change_remote_mode().await;

        if change_result {
            HttpResponse::Ok().json("REMOTE DONE")
        }

        else {
            HttpResponse::BadRequest().json("REMOTE FAILED")
        }
    }

    else {
        HttpResponse::Unauthorized().json("Not Allowed")
    }   

    
    
}