import cv2
import numpy as np
    
# 클래스 레이블 (사람 인식을 위해서만 사용)
CLASSES = ["background", "aeroplane", "bicycle", "bird", "boat",
           "bottle", "bus", "car", "cat", "chair", "cow", "diningtable",
           "dog", "horse", "motorbike", "person", "pottedplant", "sheep",
           "sofa", "train", "tvmonitor"]    
    
class AIModule:
    def __init__(self) -> None:
        
        prototxt = "D:/github/BCDelivery/app/core/models/ai_models/mobilenet/deploy.prototxt"
        model = "D:/github/BCDelivery/app/core/models/ai_models/mobilenet/mobilenet_iter_73000.caffemodel"
        self.net = cv2.dnn.readNetFromCaffe(prototxt, model)
        
        # 추적기 초기화
        self.tracker = None
        self.tracking : bool = False
    
        self.height : float = 0.0
        self.width : float = 0.0
        

    def tracking_image(self, frame):
        h, w = frame.shape[:2]

        if not self.tracking:
            # 사람 탐지
            blob = cv2.dnn.blobFromImage(cv2.resize(frame, (300, 300)), 0.007843, (300, 300), 127.5)
            self.net.setInput(blob)
            detections = self.net.forward()

            # 최고 확률의 사람 탐지 결과 찾기
            for i in range(detections.shape[2]):
                confidence = detections[0, 0, i, 2]

                if confidence > 0.5:  # 50% 이상의 확률로 사람으로 인식한 경우
                    idx = int(detections[0, 0, i, 1])
                    if CLASSES[idx] == "person":
                        # print("Human Detected")
                        box = detections[0, 0, i, 3:7] * np.array([w, h, w, h])
                        (startX, startY, endX, endY) = box.astype("int")

                        # 추적기 초기화
                        self.tracker = cv2.TrackerKCF_create()  # KCF 추적기 사용
                        self.tracking = self.tracker.init(frame, (startX, startY, endX - startX, endY - startY))

                        # 초기 박스를 그립니다.
                        cv2.rectangle(frame, (startX, startY), (endX, endY), (0, 0, 255), 2)
                        break
        else:
            # 추적 모드
            self.tracking, bbox = self.tracker.update(frame)
            if self.tracking:
                # print("Human Tracked")
                (x, y, w, h) = [int(v) for v in bbox]
                cv2.rectangle(frame, (x, y), (x + w, y + h), (0, 0, 255), 2)
            else:
                # 추적 실패 시 재탐지
                self.tracking = False        

        return frame