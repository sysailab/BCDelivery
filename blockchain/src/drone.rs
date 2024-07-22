use tokio::net::UdpSocket;
use tokio::sync::{mpsc, oneshot, Mutex};
use once_cell::sync::Lazy;
use tokio::time::{sleep, Duration};
use std::sync::Arc;
use serde_json::{json, to_string, Value};
use actix::{Actor, StreamHandler};
use actix_web::{web, App, HttpResponse, HttpServer, Responder};
use actix_web_actors::ws;

use crate::instance::config::Location;
use crate::remote::MYLOCATION;

pub static TELLO: Lazy<Arc<Mutex<Tello>>> = Lazy::new(|| {
    Arc::new(Mutex::new(Tello::default()))
});

#[derive(Default)]
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
    video_buffer: Vec<u8>,  // Vec<u8> used directly
}

impl Tello {
    pub async fn new(drone_id: String, drone_ip: String, cmd_port: u16, state_port: u16, video_port: u16) -> Self {
        let state_socket = UdpSocket::bind(("0.0.0.0", state_port)).await.expect("Couldn't bind to state port");
        let video_socket = UdpSocket::bind(("0.0.0.0", video_port)).await.expect("Couldn't bind to video port");
        let cmd_socket = UdpSocket::bind(("0.0.0.0", cmd_port)).await.expect("Couldn't bind to cmd port");

        let (cmd_sender, cmd_receiver) = mpsc::channel(32);

        let mut tello = Tello {
            drone_id,
            drone_ip,
            cmd_port,
            state_port,
            video_port,
            cmd_sender,
            state_socket: Arc::new(state_socket),
            video_socket: Arc::new(video_socket),
            cmd_socket: Arc::new(cmd_socket),
            video_buffer: Vec::new(),
        };

        Tello::start_receivers(&mut tello, cmd_receiver).await;

        tello
    }

    async fn start_receivers(tello: &mut Tello, cmd_receiver: mpsc::Receiver<String>) {
        let state_socket_clone = tello.state_socket.clone();
        let video_socket_clone = tello.video_socket.clone();
        let cmd_socket_clone = tello.cmd_socket.clone();

        tokio::spawn(async move {
            Tello::state_receiver_loop(state_socket_clone).await;
        });

        tokio::spawn(async move {
            Tello::command_receiver_loop(cmd_socket_clone).await;
        });

        tokio::spawn(async move {
            Tello::video_stream_loop(video_socket_clone, &mut tello.video_buffer).await;
        });

        tokio::spawn(async move {
            Tello::command_sender_loop(cmd_receiver, tello.drone_ip.clone(), cmd_socket_clone, tello.cmd_port).await;
        });
    }

    async fn command_sender_loop(mut receiver: mpsc::Receiver<String>, drone_ip: String, cmd_socket: Arc<UdpSocket>, cmd_port: u16) {
        while let Some(cmd) = receiver.recv().await {
            let send_result = cmd_socket.send_to(cmd.as_bytes(), (drone_ip.as_str(), cmd_port)).await;
            if let Err(e) = send_result {
                println!("Failed to send command: {}", e);
            }
        }
    }

    async fn state_receiver_loop(state_socket: Arc<UdpSocket>) {
        let mut buf = [0; 1024];
        loop {
            if let Ok((n, _)) = state_socket.recv_from(&mut buf).await {
                if let Ok(state_str) = std::str::from_utf8(&buf[..n]) {
                    println!("Received state: {}", state_str);
                }
            }
        }
    }

    async fn command_receiver_loop(cmd_socket: Arc<UdpSocket>) {
        let mut buf = [0; 1024];
        loop {
            if let Ok((n, _)) = cmd_socket.recv_from(&mut buf).await {
                if let Ok(cmd_str) = std::str::from_utf8(&buf[..n]) {
                    println!("Received command response: {}", cmd_str);
                }
            }
        }
    }

    async fn video_stream_loop(video_socket: Arc<UdpSocket>, video_buffer: &mut Vec<u8>) {
        let mut buf = [0; 2048];
        loop {
            if let Ok((n, _)) = video_socket.recv_from(&mut buf).await {
                video_buffer.clear();
                video_buffer.extend_from_slice(&buf[..n]);
            }
        }
    }

    pub async fn send_command(&self, cmd: String) {
        self.cmd_sender.send(cmd).await.expect("Failed to send command");
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
