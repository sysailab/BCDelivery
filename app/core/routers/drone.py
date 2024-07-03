from os.path import dirname, abspath
from pathlib import Path
from fastapi import APIRouter, Depends, Request, responses, status
from ..models.tello import Tello
import json
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

# db_manager.init_manager(MONGODB_URL, "simulverse")
# BASE_DIR = dirname(dirname(abspath(__file__)))

# templates = Jinja2Templates(directory=str(Path(BASE_DIR, 'templates')))

ip_table_dict = open("app/core/routers/drone_ip_table.json", 'r')
ip_table_dict = json.load(ip_table_dict)


@router.get("/control/{drone_id}/{cmd}")
def control(request: Request, drone_id: str, cmd: str):
    
    if drone_id in drones:
        drones[drone_id].command(cmd)
        return {"msg" : "ok", "cmd": cmd, "state": "default model"}
        
    else:
        
        try:
            drone_ip = ip_table_dict[drone_id]["ip_address"]
            
        except Exception as e:
            return ("No Drone IP Found")
            
        
        drones[drone_id] = Tello(drone_ip, 8889, 8890, 11111)
        drones[drone_id].command(cmd)
    
        return {"msg" : "ok", "cmd": cmd, "state": "create new model"}
    
@router.delete("/drone")
    
@router.post("/create")
def create(request: Request):
    pass


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