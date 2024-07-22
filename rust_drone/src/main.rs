mod drone;
use tokio::time::{sleep, Duration};
// static TELLO: Lazy<Mutex<Option<Arc<Tello>>>> = Lazy::new(|| Mutex::new(None));

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

    let mut is_ok: bool = false;

    loop{

        if (!is_ok)
        {   
            is_ok = true;

            tello.send_command("command".to_string()).await;
            // // println!("After Command");
            sleep(Duration::from_secs(3)).await;
    
            tello.send_command("streamon".to_string()).await;
            // // println!("After Command");
            sleep(Duration::from_secs(3)).await;
        }

        else if (is_ok)
        {
            if let Some(frame) = tello.get_latest_video_frame() {
                println!("Latest video frame: {} bytes", frame.len());
            }

            sleep(Duration::from_secs(1)).await;
        }

        



        // tello.send_command("command".to_string()).await;
        // // // println!("After Command");
        // sleep(Duration::from_secs(3)).await;

        // tello.send_command("streamon".to_string()).await;
        // // // println!("After Command");
        // sleep(Duration::from_secs(3)).await;

        // tello.send_command("streamoff".to_string()).await;
        // // // println!("After Command");
        // sleep(Duration::from_secs(3)).await;  


        // tello.send_command("takeoff".to_string()).await;

        // sleep(Duration::from_secs(3)).await;

        // tello.send_command("land".to_string()).await;
        
        // sleep(Duration::from_secs(20)).await;
    }
}
