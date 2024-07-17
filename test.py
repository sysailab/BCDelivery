import asyncio
# import aioconsole

async def producer(queue):
    input_text = 1
    while input_text < 10:
        # input_text = await aioconsole.ainput("Enter something: ")
        # input_text = 1
        await queue.put(input_text)
        # await queue.put(input_text)
        # await queue.put(input_text)
        input_text += 1
        await asyncio.sleep(1)

async def consumer_fun(queue, task_id):
    while True:
        input_text = await queue.get()
        await perform_task(task_id, input_text)
        queue.task_done()

async def perform_task(task_id, input_text):
    print(f"Task {task_id} started with input: {input_text}")
    total = 0
    for i in range(101):
        total += i
        await asyncio.sleep(10 / 100)  # Adjusting sleep to simulate the 10-second duration
    print(f"Task {task_id} completed. Total sum: {total}")

async def main():
    queue = asyncio.Queue()
    
    producers = [asyncio.create_task(producer(queue))]
    consumers = [asyncio.create_task(consumer_fun(queue, i)) for i in range(1, 4)]
    
    await asyncio.gather(*producers)
    print("Hello")
    await queue.join()  # Wait until all items in the queue have been processed
    print("Bello")
    for consumer in consumers:
        consumer.cancel()  # Cancel consumer tasks

if __name__ == "__main__":
    asyncio.run(main())
