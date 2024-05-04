use anyhow::Result;
use obws::{responses::{recording::RecordStatus, streaming::StreamStatus}, Client};
use tokio::io::{AsyncReadExt, AsyncWriteExt};
use std::{env, io};
use once_cell::sync::Lazy;

mod structs;

pub static ARGS: Lazy<Vec<String>> = Lazy::new(|| {
    let args: Vec<String> = env::args().collect();
    args
});

#[tokio::main]
async fn main() {
    let mut password = String::new();
    println!("Enter OBS password:");

    match io::stdin().read_line(&mut password) {
        Ok(_) => {
            password = password.trim_end().to_string();
        }
        Err(error) => println!("Error: {}", error),
    }

    let mut output: structs::output = structs::output {
        stream: None,
        recording: None
    };

    let is_streaming = env::args().any(|arg| arg == "--streaming");
    if is_streaming {
        let stream = get_streaming(password.clone()).await.expect("Missing stream");
        output.stream = Some(stream);
    }

    let is_recording = env::args().any(|arg| arg == "--recording");
    if is_recording {
        let recording = get_recording(password.clone()).await.expect("Missing recording");
        output.recording = Some(recording);
    }

    let serialized = serde_json::to_string(&output).unwrap();

    println!("OUTPUT: {}", serialized);
}

async fn get_client(password: String) -> Result<obws::Client, anyhow::Error> {
    Ok(Client::connect("localhost", 4455, Some(password)).await?)
}

async fn get_streaming(password: String) -> Result<StreamStatus> {
    let client = get_client(password).await?;

    // Get a list of available scenes and print them out.
    let stream_status = client.streaming().status().await?;

    Ok(stream_status)
}

async fn get_recording(password: String) -> Result<RecordStatus> {
    let client = get_client(password).await?;

    // Get a list of available scenes and print them out.
    let recording_status = client.recording().status().await?;

    Ok(recording_status)
}