from os.path import dirname, abspath
from pathlib import Path
from fastapi import APIRouter, Depends, Request, responses, status, Response
from ..models.tello import Tello
import json
from ..models.base_model import DroneControl
from ..models.dict_scheduler import DictScheduler
import queue
import io
from starlette.responses import StreamingResponse
from .drone_command import *


router = APIRouter()

# drones = DictScheduler(timeout=3600, check_interval=60)
drones = {}

# drone_rep_queue = queue.Queue()

# drone_video_queue = queue.Queue(maxsize=1)


ip_table_dict = open("app/core/routers/drone_ip_table.json", 'r')
ip_table_dict = json.load(ip_table_dict)    
    
@router.post("/control/")
def control(request: Request, drone_control: DroneControl):
    """드론 원격 제어를 위한 코드

    Args:
        drone_id (str): 드론 ID
        cmd (str): 제어문

    Returns:
        _type_: _description_
    """
        
    if drone_control.drone_id in drones:

        drones[drone_control.drone_id].command(drone_control.cmd)
        
        drone_rep = drones[drone_control.drone_id].drone_rep_queue.get()
        
        if drone_rep == 0:
            return {"status": "ok", "msg" : f"{drone_control.cmd} Commanded."}
        
        elif drone_rep == 1:
            return {"status": "err", "msg" : f"{drone_control.drone_id} Drone did not responsed."}
            
        else:
            return {"status ": "except", "msg" : f"{drone_rep}"}
        
    else:

        if not drone_initialize(drone_control.drone_id):
            return {"status": "err", "msg" : f"No Drone IP Found."}
        
        # try:
        #     drone_ip = ip_table_dict[drone_control.drone_id]["ip_address"]
                    
        # except Exception as e:
        #     return {"status": "err", "msg" : f"No Drone IP Found."}
        
        # drone_cmd_port = ip_table_dict[drone_control.drone_id]["ports"]["command"]
        # drone_state_port = ip_table_dict[drone_control.drone_id]["ports"]["state"]
        # drone_video_port = ip_table_dict[drone_control.drone_id]["ports"]["video"]

        
        
        # drones[drone_control.drone_id] = Tello(drone_control.drone_id, drone_ip, \
        #     drone_cmd_port, drone_state_port, drone_video_port)
        
        drones[drone_control.drone_id].command(drone_control.cmd)
        
        drone_rep = drones[drone_control.drone_id].drone_rep_queue.get()
        
        if drone_rep == 0:
            return {"status" : "ok", "msg": f"{drone_control.drone_id} : New Model Created. {drone_control.cmd} Commanded."}   
        
        elif drone_rep == 1:
            return {"status": "err", "msg" : f"Drone did not responsed."}     
        
        else:
            return {"status ": "except", "msg" : f"{drone_rep}"}           
    
    
    
@router.delete("/{drone_id}")
def delete_drone(request: Request, drone_id: str):
    if drone_id in drones:
        
        del drones[drone_id]
        return {"status" : "ok", "msg": f"{drone_id} Deleted."} 
    
    else:
        return {"status" : "err", "msg": f"No {drone_id} Found."} 
    
@router.get("/video/{drone_id}")
def stream_video(request: Request, drone_id: str):
    
    if drone_id in drones:
        try:
            frame = drones[drone_id].drone_video_queue.get_nowait()
            return StreamingResponse(io.BytesIO(frame), media_type="image/jpeg")
        
        except queue.Empty:
            return Response(content="No frame available. Maybe Drone is turned off.", status_code=500)
        
    else:     
        if not drone_initialize(drone_id):
            return {"status": "err", "msg" : f"No Drone IP Found."}        
    
        try:        
            drones[drone_id].command(DRONE_COMMAND)
            if drones[drone_id].drone_rep_queue.get() == 1:
                return {"status": "err", "msg" : f"{drone_id} Drone did not responsed."}

            drones[drone_id].command(DRONE_STREAMON)
            drones[drone_id].drone_rep_queue.get()
            
            # frame = drones[drone_id].drone_video_queue.get_nowait()
            frame = drones[drone_id].drone_video_queue.get()
            
            return StreamingResponse(io.BytesIO(frame), media_type="image/jpeg")
            
        except:
            # print("err")
            return Response(content= "No frame available. Maybe Drone is turned off", status_code=500)
            
        # return Response(content= "임시 에러", status_code=500)
    
    
def drone_initialize(_drone_id) -> Tello:
    try:
        drone_ip = ip_table_dict[_drone_id]["ip_address"]
                
    except Exception as e:
        return False
    
    drone_cmd_port = ip_table_dict[_drone_id]["ports"]["command"]
    drone_state_port = ip_table_dict[_drone_id]["ports"]["state"]
    drone_video_port = ip_table_dict[_drone_id]["ports"]["video"]
    
    drones[_drone_id] = Tello(_drone_id, drone_ip, \
                drone_cmd_port, drone_state_port, drone_video_port)
    
    return True
    