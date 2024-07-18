mod drone;
use tokio::time::{sleep, Duration};


#[tokio::main]
async fn main() {
    let tello = drone::Tello::new(
        "tt-16".to_string(),
        "192.168.50.13".to_string(),
        8889,
        8890,
        11111,
    )
    .await;

    // loop{

        // tello.send_command("command".to_string()).await;
        // // println!("After Command");
        // sleep(Duration::from_secs(3)).await;

        // tello.send_command("takeoff".to_string()).await;

        // sleep(Duration::from_secs(3)).await;

        tello.send_command("land".to_string()).await;
    // }
    
}
