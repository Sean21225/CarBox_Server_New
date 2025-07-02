
import * as React from 'react';
import './SearchBox.css';
import AzureMap from './AzureMap';

function Map(props) {
  
  const userLocation =  props.userLocation; 
  const stations = props.stations
  


  return (
        <div className='flex h-screen'>
        <div>
        <AzureMap subscriptionKey={'BYG1g5auep5jrbjPOjn34Btd5vK9Yet9YsZEcHWm3yuMNammODmqJQQJ99BBACYeBjF5VmYeAAAgAZMP1IvB'} stations={stations} userLocation={userLocation}/>
        </div>
        </div>         
        );
  
}

export default Map; 


