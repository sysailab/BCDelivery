use tokio::net::UdpSocket;
use tokio::sync::{mpsc, oneshot};
use tokio::task;
use tokio::time::{sleep, Duration};
use std::sync::{Arc, Mutex};

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
    latest_video_frame: Arc<Mutex<Option<Vec<u8>>>>
}

impl Tello {
    pub async fn new(drone_id: String, drone_ip: String, cmd_port: u16, state_port: u16, video_port: u16) -> Self {
        let state_socket = UdpSocket::bind(("0.0.0.0", state_port)).await.expect("Couldn't bind to state port");
        let video_socket = UdpSocket::bind(("0.0.0.0", video_port)).await.expect("Couldn't bind to video port");
        let cmd_socket: UdpSocket = UdpSocket::bind(("0.0.0.0", cmd_port)).await.expect("Couldn't bind to cmd port");

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
            latest_video_frame: Arc::new(Mutex::new(None)),
        };
        
        let cmd_socket_clone: Arc<UdpSocket> = tello.cmd_socket.clone();
        let state_socket_clone = tello.state_socket.clone();
        let video_socket_clone = tello.video_socket.clone();
        let video_frame_clone = tello.latest_video_frame.clone();


        tokio::spawn(async move {
            Tello::command_sender_loop(cmd_receiver, drone_ip, cmd_socket_clone, cmd_port).await;
        });

        tokio::spawn(async move {
            Tello::state_receiver_loop(state_socket_clone).await;
        });

        tokio::spawn(async move {
            Tello::video_stream_loop(video_socket_clone, video_frame_clone).await;
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
                            println!("Received state from {}: {}", addr, state_str);
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


    async fn video_stream_loop(video_socket: Arc<UdpSocket>, video_frame: Arc<Mutex<Option<Vec<u8>>>>)  {
        let mut buf = [0; 2048];

        loop {
            match video_socket.recv_from(&mut buf).await {
                Ok((n, addr)) => {
                    // println!("Received video from {}: {} bytes", addr, n);
                    // Process video frame here
                    let mut frame = video_frame.lock().unwrap();
                    *frame = Some(buf[..n].to_vec());
                }
                Err(e) => {
                    println!("Failed to receive video: {}", e);
                }
            }
        }
    }

    pub fn get_latest_video_frame(&self) -> Option<Vec<u8>> {
        let frame = self.latest_video_frame.lock().unwrap();
        frame.clone()
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
