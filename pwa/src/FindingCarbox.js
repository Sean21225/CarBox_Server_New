
import * as React from 'react';
import './FindingCarbox.css';
import CarboxCard from './CarboxCard';
import { useLocation} from "react-router-dom";

function FindingCarbox() {
  // const location = useLocation();
  // const originStation = location.state.originStation;
  // const destinationStation = location.state.destinationStation;
  // const departureTime = location.state.departureTime;
  const originStation = 'A'
  const destinationStation = 'C'
  const departureTime = "16:00"




    // list of carbox from the server - and card for one carbox
  return (
    <div>
    <div id='carbox-list'>
    We found a carbox for you
    
    <CarboxCard className="grid-item" id='A1' origin={originStation} destination={destinationStation} departureTime={departureTime} arrivalTime='16:15'/>
    </div>
    </div>
  );
  
}

export default FindingCarbox; 


