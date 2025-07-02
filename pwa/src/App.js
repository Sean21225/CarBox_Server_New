
import * as React from 'react';
import Button from '@mui/material/Button';
import { useState } from 'react';
import './App.css';

function App() {
  const [status, setStatus] = useState('');

  const handleStart = () => {
    setStatus("start")
    fetch("https://localhost:7158/api/StartStop", {
      method: "POST", 
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(
        {
          status: "start"
        }
      )
    })
    .then((response) => {
        if (!response.ok) {
          throw new Error(`Server error: ${response.status}`);
        }
        return response.json();
      })
      .then((data) => {
        console.log("Status updated:", data);
      })
      .catch((error) => {
        console.error("Fetch error:", error);
      });
  //     .then((response) => response.json())
  //     .then((data) => {
  //     })
  //     .catch((error) => console.log(error));
   } ;

  const handleStop = () => {
    setStatus("stop")
    fetch("https://localhost:7158/api/StartStop", {
      method: "POST", 
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(
        {
          status: "stop"
        }
      )
    })
    .then((response) => {
      if (!response.ok) {
        throw new Error(`Server error: ${response.status}`);
      }
      return response.json();
    })
    .then((data) => {
      console.log("Status updated:", data);
    })
    .catch((error) => {
      console.error("Fetch error:", error);
    });
  };


  return (
    <div >
    <button onClick={handleStart}>starttt</button>
    <Button variant="contained" onClick={handleStop}>stoppp</Button>
    </div>
  );
  
}

export default App;


