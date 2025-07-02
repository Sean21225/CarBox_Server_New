
import * as React from 'react';
import './CarboxCard.css';
import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap-icons/font/bootstrap-icons.css';
import Card from 'react-bootstrap/Card';
import Button from 'react-bootstrap/Button';

function CarboxCard(props) {
  const id = props.id
  const origin = props.origin 
  const destination = props.destination 
  const departureTime = props.departureTime 
  const arrivalTime = props.arrivalTime 

  return (
    
    <Card>

      <Card.Header>Departure Time: <strong>{departureTime}</strong> | Arrival Time: <strong>{arrivalTime}</strong></Card.Header>
      <Card.Body>
        <Card.Title>CARBOX <strong>{id}</strong></Card.Title>
        <Card.Text>
        
          {origin} <i class="bi bi-arrow-right"></i> {destination}
        </Card.Text>
        <Button variant="primary">START</Button>
      </Card.Body>
    </Card>
  );
  
}

export default CarboxCard; 


