
import * as React from 'react';
import './CarboxArrived.css';
import 'bootstrap/dist/css/bootstrap.min.css';
import Button from 'react-bootstrap/Button';

function CarboxArrived(props) {

  return (
    
    <div id='arrived'>
    <div id='arrived-message'>
    Your car has<br></br> 
    arrived at the station
    </div>
    <Button variant="primary">Start now</Button>
    </div>
  );
  
}

export default CarboxArrived; 


