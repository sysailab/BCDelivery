use tokio::net::UdpSocket;
use tokio::sync::{mpsc, oneshot, Mutex};
use once_cell::sync::Lazy;
use tokio::task;
use tokio::time::{sleep, Duration};
use std::sync::Arc;
use serde_json::{json, to_string, Value};
use actix::{Actor, StreamHandler};
use actix_web::{web, App, HttpResponse, HttpServer, Responder};
use actix_web_actors::ws;

use crate::instance::config::Location;
use crate::remote::MYLOCATION;

pub static TELLO: Lazy<Arc<Mutex<Option<Tello>>>> = Lazy::new(|| {
    Arc::new(Mutex::new(None))
});

pub struct Tello {
    drone_id: String,
    drone_ip: String,
    cmd_port: u16,
    state_port: u16,
    video_port: u16,
    cmd_sender: mpsc::Sender<String>,
    state_socket: Arc<UdpSocket>,
    video_socket: Arc<UdpSocket>,
    cmd_socket: Arc<UdpSocket>,
    pub video_buffer: Arc<Mutex<Vec<u8>>>
}

impl Tello {
    pub async fn new(drone_id: String, drone_ip: String, cmd_port: u16, state_port: u16, video_port: u16) -> Self {
        let state_socket = UdpSocket::bind(("0.0.0.0", state_port)).await.expect("Couldn't bind to state port");
        let video_socket = UdpSocket::bind(("0.0.0.0", video_port)).await.expect("Couldn't bind to video port");
        let cmd_socket: UdpSocket = UdpSocket::bind(("0.0.0.0", cmd_port)).await.expect("Couldn't bind to cmd port");
        let video_buffer = Arc::new(Mutex::new(Vec::new()));

        let (cmd_sender, cmd_receiver) = mpsc::channel(32);

        let tello = Tello {
            drone_id,
            // drone_ip: drone_ip.clone(),
            drone_ip: drone_ip.clone(),
            cmd_port,
            state_port,
            video_port,
            cmd_sender,
            state_socket: Arc::new(state_socket),
            video_socket: Arc::new(video_socket),
            cmd_socket:  Arc::new(cmd_socket),
            video_buffer: video_buffer.clone()
        };
        
        let cmd_socket_clone: Arc<UdpSocket> = tello.cmd_socket.clone();
        let state_socket_clone = tello.state_socket.clone();
        let video_socket_clone = tello.video_socket.clone();

        tokio::spawn(async move {
            Tello::command_sender_loop(cmd_receiver, drone_ip, cmd_socket_clone, cmd_port).await;
        });

        tokio::spawn(async move {
            Tello::state_receiver_loop(state_socket_clone).await;
        });

        tokio::spawn(async move {
            Tello::video_stream_loop(video_socket_clone, video_buffer.clone()).await;
        });
        
        let cmd_socket_clone = tello.cmd_socket.clone();

        tokio::spawn(async move {
            Tello::command_receiver_loop(cmd_socket_clone).await;
        });

        tello
    }

    pub async fn send_command(&self, cmd: String) {
        self.cmd_sender.send(cmd).await.expect("Failed to send command");
    }

    async fn command_sender_loop(mut receiver: mpsc::Receiver<String>, drone_ip: String, cmd_socket: Arc<UdpSocket>, cmd_port: u16) {
        // let cmd_socket = UdpSocket::bind("0.0.0.0:0").await.expect("Couldn't bind to command port");

        while let Some(cmd) = receiver.recv().await {
            println!("Sending command: {}", cmd);

            let mut success = false;
            cmd_socket.send_to(cmd.as_bytes(), (drone_ip.as_str(), cmd_port)).await.expect("Failed to send command");
            for _ in 0..3 {
                cmd_socket.send_to(cmd.as_bytes(), (drone_ip.as_str(), cmd_port)).await.expect("Failed to send command");

                // Wait for response
                let mut buf = [0; 1024];
                match cmd_socket.recv_from(&mut buf).await {
                    Ok((_, _)) => {
                        success = true;
                        break;
                    }
                    Err(_) => {
                        println!("Retrying...");
                    }
                }

                sleep(Duration::from_secs(5)).await;
            }

            if success {
                println!("Command sent successfully");
            } else {
                println!("Failed to send command");
            }
        }
    }

    async fn state_receiver_loop(state_socket: Arc<UdpSocket>) {
        let mut buf = [0; 1024];

        loop {
            match state_socket.recv_from(&mut buf).await {
                Ok((n, addr)) => {
                    match std::str::from_utf8(&buf[..n]) {
                        Ok(state_str) => {
                            println!("Received state from {}: {}", addr, state_str.clone());
                            let mut x= String::new();
                            let mut y= String::new();
                            let mut z= String::new();
                            
                            for entry in state_str.split(';') {
                                let parts: Vec<&str> = entry.split(':').collect();
                                if parts.len() == 2 {
                                    match parts[0] {
                                        "x" => x = parts[1].to_string(),
                                        "y" => y = parts[1].to_string(),
                                        "z" => z = parts[1].to_string(),
                                        _ => {}
                                    }
                                }
                            }

                            {
                                let new_location = Location::new(x, y, z);
                                let mut location_lock = MYLOCATION.lock().unwrap();
                                *location_lock = new_location;
                            }
                                                        
                        }
                        Err(e) => {
                            println!("Failed to convert bytes to string: {}", e);
                        }
                    }
                }
                Err(e) => {
                    println!("Failed to receive state: {}", e);
                }
            }
        }
    }

    async fn command_receiver_loop(cmd_socket: Arc<UdpSocket>) {
        let mut buf = [0; 1024];

        loop {
            match cmd_socket.recv_from(&mut buf).await {
                Ok((n, addr)) => {
                    match std::str::from_utf8(&buf[..n]) {
                        Ok(state_str) => {
                            println!(" # Received Command Result from {}: {}", addr, state_str);
                        }
                        Err(e) => {
                            println!("Failed to convert bytes to string: {}", e);
                        }
                    }
                }
                Err(e) => {
                    println!("Failed to receive state: {}", e);
                }
            }
        }
    }


    async fn video_stream_loop(video_socket: Arc<UdpSocket>, video_buffer: Arc<Mutex<Vec<u8>>>) {
        let mut buf = [0; 2048];
        loop {
            match video_socket.recv_from(&mut buf).await {
                Ok((n, _addr)) => {
                    let mut buffer_lock = video_buffer.lock().await;
                    buffer_lock.clear();
                    buffer_lock.extend_from_slice(&buf[..n]);
                }
                Err(e) => {
                    println!("Failed to receive video: {}", e);
                }
            }
        }
    }

    
}



// #[tokio::main]
// async fn main() {
//     let tello = Tello::new(
//         "drone1".to_string(),
//         "192.168.10.1".to_string(),
//         8889,
//         8890,
//         11111,
//     )
//     .await;

//     tello.send_command("command".to_string()).await;
// }
