// blockchain.rs
use serde::{Deserialize, Serialize};
use std::time::{SystemTime, UNIX_EPOCH};
use sha2::{Sha256, Digest};

use crate::instance::config;

#[derive(Serialize, Deserialize, Debug, Clone)]
pub struct Block {
    pub index: u64,
    pub timestamp: u64,
    pub data: Vec<Data>,
    pub previous_hash: String,
    pub nonce: u64,
    pub hash: String,
}

#[derive(Serialize, Deserialize, Debug, Clone)]
pub struct Data {
    pub id: String,
    pub command: String
}

impl Block {
    pub fn new(index: u64, data: Vec<Data>, previous_hash: String) -> Self {
        Block {
            index,
            timestamp: SystemTime::now().duration_since(UNIX_EPOCH).unwrap().as_secs(),
            data,
            previous_hash,
            nonce: 0,
            hash: String::new(),
        }
    }

    // Proof of Work Algorithm
    pub fn mine_block(&mut self, difficulty: usize) {
        let target = "0".repeat(difficulty);
        self.nonce = 0;

        if self.hash.len() < difficulty {
            self.hash = "0".repeat(difficulty); // 적절한 초기값 설정
        }

        loop {
            self.hash = self.calculate_hash();
            if self.hash.starts_with(&target) {
                break;  // 조건을 만족하면 루프 종료
            }
            self.nonce += 1;  // 조건을 만족하지 않으면 nonce 증가
        }
    }

    fn calculate_hash(&self) -> String {
        let contents = format!("{}{}{:?}{}{}", self.index, self.timestamp, self.data, self.previous_hash, self.nonce);
        let mut hasher = Sha256::new();
        hasher.update(contents);
        let result = hasher.finalize();
        format!("{:x}", result)
    }
}

pub struct Blockchain {
    pub blocks: Vec<Block>,
    pub difficulty: usize,
}

impl Blockchain {
    pub fn new() -> Self {
        let mut blockchain = Blockchain {
            blocks: Vec::new(),
            difficulty: config::DIFFICULTY,
        };

        let mut genesis_data = Vec::new();

        let data = Data {
            id : "Genesis".to_owned(),
            command : "Genesis".to_owned()
        };

        genesis_data.push(data);

        let genesis_block = Block::new(0, genesis_data, "0".to_string());
        blockchain.blocks.push(genesis_block);
        blockchain
    }

    pub fn add_block(&mut self, data: Data) -> Block {
        let last_block = self.blocks.last().unwrap();
        let mut new_block_data = last_block.data.clone();
        new_block_data.push(data);

        let mut new_block = Block::new(self.blocks.len() as u64, new_block_data, last_block.hash.clone());
        new_block.mine_block(self.difficulty);
        self.blocks.push(new_block.clone());

        return new_block.clone();
    }

    pub fn update_block(&mut self, block: Block) {
        self.blocks.push(block);
    }

    pub fn is_valid(&self, block: &Block) -> bool {
        for i in 1..self.blocks.len() {
            let current = &self.blocks[i];
            let previous = &self.blocks[i-1];

            if current.hash != Block::calculate_hash(block) {
                return false;
            }

            if current.previous_hash != previous.hash {
                return  false;
            }
        }
        
        true
    }

    pub fn get_last_block(&self) -> &Block {
        self.blocks.last().unwrap()
    }

    // pub fn add_new_transaction(&mut self, tra)


    
}
