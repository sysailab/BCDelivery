from djitellopy import Tello
import time

# Tello 객체 생성
tello = Tello()

# 드론 연결
tello.connect()

# 배터리 상태 확인
battery_level = tello.get_battery()
print(f"Battery level: {battery_level}%")

# 이륙
tello.takeoff()

# 앞으로 100cm 이동
tello.move_forward(100)

# 90도 회전
tello.rotate_clockwise(90)

# 뒤로 100cm 이동
tello.move_back(100)

# 착륙
tello.land()

# 드론 연결 해제
tello.end()
