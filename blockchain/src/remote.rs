use tokio::time::{sleep, Duration};

use crate::drone::{self, Tello, TELLO};
use crate::instance::config::{CMD_PORT, NODE_TYPE, STATE_PORT, VIDEO_PORT, Location};

use std::sync::Mutex;

use lazy_static::lazy_static;

lazy_static! {
    pub static ref MYLOCATION: Mutex<Location> = Mutex::new(Location::new("00.00".to_owned(), "00.00".to_owned(), "00.00".to_owned()));
}

pub async fn init_tello(drone_id: String, drone_ip: String, cmd_port: u16, state_port: u16, video_port: u16) {
    let tello = Tello::new(drone_id, drone_ip, cmd_port, state_port, video_port).await;
    let mut tello_lock = TELLO.lock().await;

    *tello_lock = Some(tello);
}

pub async fn cmd_start(cmd: String, node_type: String) {
    let tello_lock = TELLO.lock().await;
    if let Some(tello) = &*tello_lock {

        /*
        Usable Command
        takeoff, land, streamon, streamoff
        */

        // DEBUG
        tello.send_command("command".to_string()).await;
        // // println!("After Command");
        sleep(Duration::from_secs(3)).await;

        tello.send_command("streamon".to_string()).await;
        // // println!("After Command");
        sleep(Duration::from_secs(3)).await;

        tello.send_command("streamoff".to_string()).await;
        // // println!("After Command");
        sleep(Duration::from_secs(3)).await;  


        tello.send_command("takeoff".to_string()).await;

        sleep(Duration::from_secs(3)).await;

        tello.send_command("land".to_string()).await;
        
        sleep(Duration::from_secs(20)).await;

        // tello.send_command("command".to_owned()).await;
        // tello.send_command(cmd).await;
    }
}
