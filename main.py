import uvicorn
import sys

if __name__ == '__main__':
        uvicorn.run("app.main:app",
                host="0.0.0.0",
                port=17148,
                reload=True,
                )