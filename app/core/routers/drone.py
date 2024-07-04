from os.path import dirname, abspath
from pathlib import Path
from fastapi import APIRouter, Depends, Request, responses, status, Response
from ..models.tello import Tello
import json
from ..models.base_model import DroneControl
import queue
import io
from starlette.responses import StreamingResponse
# from fastapi.templating import Jinja2Templates
# from fastapi.staticfiles import StaticFiles
# from ..models.database import db_manager
# from ..instance.config import MONGODB_URL, ACCESS_TOKEN_EXPIRE_MINUTES

# from ..schemas.user_model import UserRegisterForm, UserModel

router = APIRouter()

drones = {}

drone_rep_queue = queue.Queue()

drone_video_queue = queue.Queue(maxsize=1)


ip_table_dict = open("app/core/routers/drone_ip_table.json", 'r')
ip_table_dict = json.load(ip_table_dict)

# @router.get("/control/{drone_id}/{cmd}")
# def control(request: Request, drone_id: str, cmd: str):
#     """드론 원격 제어를 위한 코드

#     Args:
#         drone_id (str): 드론 ID
#         cmd (str): 제어문

#     Returns:
#         _type_: _description_
#     """
    
#     if drone_id in drones:
#         drones[drone_id].command(cmd)
#         # drone_rep_queue.get()
        
#         return {"status": "ok", "msg" : f"{cmd} Commanded."}
        
#     else:
        
#         try:
#             drone_ip = ip_table_dict[drone_id]["ip_address"]
            
#         except Exception as e:
#             return {"status": "err", "msg" : f"No Drone IP Found."}
            
            
        
#         drones[drone_id] = Tello(drone_id, drone_ip, 8889, 8890, 11111, drone_rep_queue)
#         drones[drone_id].command(cmd)
        
#         return {"status" : "ok", "msg": f"{drone_id} : New Model Created. {cmd} Commanded."}
    
    
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
        
        drone_rep = drone_rep_queue.get()
        
        if drone_rep == 0:
            return {"status": "ok", "msg" : f"{drone_control.cmd} Commanded."}
        
        elif drone_rep == 1:
            return {"status": "err", "msg" : f"Drone did not responsed."}
            
        else:
            return {"status ": "except", "msg" : f"{drone_rep}"}
        
    else:
        
        try:
            drone_ip = ip_table_dict[drone_control.drone_id]["ip_address"]
                    
        except Exception as e:
            return {"status": "err", "msg" : f"No Drone IP Found."}
        
        drone_cmd_port = ip_table_dict[drone_control.drone_id]["ports"]["command"]
        drone_state_port = ip_table_dict[drone_control.drone_id]["ports"]["state"]
        drone_video_port = ip_table_dict[drone_control.drone_id]["ports"]["video"]

        
        
        drones[drone_control.drone_id] = Tello(drone_control.drone_id, drone_ip, \
            drone_cmd_port, drone_state_port, drone_video_port, drone_rep_queue, drone_video_queue)
        
        drones[drone_control.drone_id].command(drone_control.cmd)
        
        drone_rep = drone_rep_queue.get()
        
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
    
@router.get("/video")
def stream_video(request: Request):
    
    try:
        frame = drone_video_queue.get_nowait()
        return StreamingResponse(io.BytesIO(frame), media_type="image/jpeg")
    
    except queue.Empty:
        return Response(content="No frame available", status_code=500)



# @router.post("/create")
# def create(request: Request):
#     pass


# @router.get("/register/")
# def render_register(request: Request):
#     return templates.TemplateResponse("auth/register.html", {"request": request})

# @router.post("/register/")
# async def handle_register(request: Request):
#     form = UserRegisterForm(request)
#     await form.load_data()
#     if await form.is_valid():
#         if await db_manager.create_user(form):
#             return responses.RedirectResponse(
#                 "/?msg=Successfully-Registered", status_code=status.HTTP_302_FOUND
#             )  # default is post request, to use get request added status code 302
#         else:
#             form.__dict__.get("errors").append("Duplicate username or email")
#             return templates.TemplateResponse("auth/register.html", form.__dict__)
            
#     return templates.TemplateResponse("auth/register.html", form.__dict__)