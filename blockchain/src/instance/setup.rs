use std::env;
use std::io::{self};
use crate::{instance::config::{self, NODE_TYPE, REMOTEIP}, IPADDR, PORT};

use super::config::REMOTEMODE;

pub fn check_if_container() -> String {
    let container_name = env::var("CONTAINER_NAME").unwrap_or_else(|_| "Unknown".to_string());
    container_name
}

pub fn container_setting() -> String {
    let container_port = env::var("PORT").unwrap_or_else(|_| "Unknown".to_string());
    container_port
}

pub async fn setup_mode() -> (String, String) {
    let mut my_ip = IPADDR.lock().unwrap();
    let mut my_port = PORT.lock().unwrap();

    let container_name = check_if_container();
    
    if container_name != "Unknown" {
        println!("Change to Container mode");

        let ip_name = container_name;
        let mut port = container_setting();

        if port == "Unknown" {
            port = config::GENESIS_PORT.to_owned();
            println!("PORT ENV NOT FOUND PORT SETUP TO 9999");
        }

        *my_ip = ip_name;
        *my_port = port;
    }

    else {
        println!("Local MODE");
    }

    return (my_ip.clone(), my_port.clone());
}

pub async fn clear_remote_mode() {
    let mut remote_mode_lock = REMOTEMODE.lock().unwrap();

    *remote_mode_lock = false;
}

pub fn local_node_setup() {
    let mut input_type = String::new();
    println!("Enter NODE TPYE");
    io::stdin().read_line(&mut input_type).expect("INPUT TYPE ERROR");
    let input_type = input_type.trim();

    let mut remote_ip = String::new();
    println!("Enter REMOTE NODE IP");
    io::stdin().read_line(&mut remote_ip).expect("REMOTE IP ERROR");
    let remote_ip = remote_ip.trim();

    {
        let mut node_type = NODE_TYPE.lock().unwrap();
        *node_type = input_type.to_string();

        let mut remote_addr = REMOTEIP.lock().unwrap();
        *remote_addr = remote_ip.to_string();
    }
}

pub fn genesis_node_setup() {
    let input_type = "Genesis";
    let remote_ip = "Genesis";

    {
        let mut node_type = NODE_TYPE.lock().unwrap();
        *node_type = input_type.to_string();

        let mut remote_addr = REMOTEIP.lock().unwrap();
        *remote_addr = remote_ip.to_string();
    }
}