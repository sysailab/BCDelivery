import robomaster
from robomaster import robot


if __name__ == '__main__':
    print("Hello")
    # robomaster.config.LOCAL_IP_STR = "192.168.50.39"
    # ep_robot.initialize(conn_type='sta')
    ep_robot = robot.Robot()

    # 指定连接方式为AP 直连模式
    # ep_robot.initialize(conn_type='sta', proto_type="tcp", ip="192.168.50.39")
    ep_robot.initialize(conn_type='sta')

    version = ep_robot.get_version()
    print("Robot version: {0}".format(version))
    
    
    ep_chassis = ep_robot.chassis
    ep_chassis.move(x=1, y=0, z=0, xy_speed=1).wait_for_completed()    
    
    ep_robot.close()
    