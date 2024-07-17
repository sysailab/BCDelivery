import robomaster
from robomaster import robot, camera
import time

if __name__ == '__main__':
    print("Hello")
    # robomaster.config.LOCAL_IP_STR = "192.168.50.39"
    # ep_robot.initialize(conn_type='sta')
    ep_robot = robot.Robot()

    # 指定连接方式为AP 直连模式
    # ep_robot.initialize(conn_type='sta', proto_type="tcp", ip="192.168.50.39")
    ep_robot.initialize(conn_type='sta', proto_type="tcp")
    print(1)
    version = ep_robot.get_version()
    print("Robot version: {0}".format(version))
    
    
    ep_chassis = ep_robot.chassis
    ep_chassis.move(x=1, y=0, z=0, xy_speed=1).wait_for_completed()    
    ep_chassis.move(x=-1, y=0, z=0, xy_speed=1).wait_for_completed()    
    
    ep_camera= ep_robot.camera
    
    data = ep_camera.start_video_stream(display=True, resolution=camera.STREAM_360P)
    print(data)
    time.sleep(10)
    ep_camera.stop_video_stream()
    
    ep_robot.close()
    
    