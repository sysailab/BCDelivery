use tokio::sync::Mutex;
use once_cell::sync::Lazy;
use std::sync::Arc;

use crate::drone::{self, Tello};
use crate::instance::config::{CMD_PORT, NODE_TYPE, STATE_PORT, VIDEO_PORT};

use lazy_static::lazy_static;

static TELLO: Lazy<Arc<Mutex<Option<Tello>>>> = Lazy::new(|| {
    Arc::new(Mutex::new(None))
});

pub async fn init_tello(drone_id: String, drone_ip: String, cmd_port: u16, state_port: u16, video_port: u16) {
    let tello = Tello::new(drone_id, drone_ip, cmd_port, state_port, video_port).await;
    let mut tello_lock = TELLO.lock().await;

    *tello_lock = Some(tello);
}

pub async fn cmd_start(cmd: String, node_type: String) {
    let tello_lock = TELLO.lock().await;
    if let Some(tello) = &*tello_lock {
        tello.send_command("command".to_owned()).await;
        tello.send_command(cmd).await;
    }
}
