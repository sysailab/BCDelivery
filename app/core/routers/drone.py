from os.path import dirname, abspath
from pathlib import Path
from fastapi import APIRouter, Depends, Request, responses, status
from ..models.tello import Tello
import json
from ..models.base_model import DroneControl
# from fastapi.templating import Jinja2Templates
# from fastapi.staticfiles import StaticFiles
# from ..models.database import db_manager
# from ..instance.config import MONGODB_URL, ACCESS_TOKEN_EXPIRE_MINUTES

# from ..schemas.user_model import UserRegisterForm, UserModel

router = APIRouter()

drones = {
    # "tt-15": Tello("192.168.50.11", 8889, 8890, 11111),
    # "tt-16": Tello("192.168.50.12", 8889, 8890, 11111),
    # "tt-17": Tello("192.168.50.13", 8889, 8890, 11111),
}

ip_table_dict = open("app/core/routers/drone_ip_table.json", 'r')
ip_table_dict = json.load(ip_table_dict)

@router.get("/control/{drone_id}/{cmd}")
def control(request: Request, drone_id: str, cmd: str):
    """드론 원격 제어를 위한 코드

    Args:
        drone_id (str): 드론 ID
        cmd (str): 제어문

    Returns:
        _type_: _description_
    """
    
    if drone_id in drones:
        drones[drone_id].command(cmd)
        return {"status": "ok", "msg" : f"{cmd} Commanded."}
        
    else:
        
        try:
            drone_ip = ip_table_dict[drone_id]["ip_address"]
            
        except Exception as e:
            return {"status": "err", "msg" : f"No Drone IP Found."}
            
            
        
        drones[drone_id] = Tello(drone_id, drone_ip, 8889, 8890, 11111)
        drones[drone_id].command(cmd)
        
        return {"status" : "ok", "msg": f"{drone_id} : New Model Created. {cmd} Commanded."}
    
    
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
        return {"status": "ok", "msg" : f"{drone_control.cmd} Commanded."}
        
    else:
        
        try:
            drone_ip = ip_table_dict[drone_control.drone_id]["ip_address"]
            
        except Exception as e:
            return {"status": "err", "msg" : f"No Drone IP Found."}
            
        drones[drone_control.drone_id] = Tello(drone_ip, 8889, 8890, 11111)
        drones[drone_control.drone_id].command(drone_control.cmd)
        
        return {"status" : "ok", "msg": f"{drone_control.drone_id} : New Model Created. {drone_control.cmd} Commanded."}    
    
    
    
@router.delete("/drone/{drone_id}")
def delete_drone(request: Request, drone_id: str):
    
    if drone_id in drones:
        
        del drones[drone_id]
        return {"status" : "ok", "msg": f"{drone_id} Deleted."} 
    
    else:
        return {"status" : "err", "msg": f"No {drone_id} Found."} 
    
    
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