
  import * as React from 'react';
  import TextField from '@mui/material/TextField';
  import Box from '@mui/material/Box';
  import Button from '@mui/material/Button';
  import { useEffect,useState } from 'react';
  import { useNavigate } from "react-router-dom";
  import './SearchBox.css';
  import Map from './Map';
  import LoadingPage from './LoadingPage';

  function SearchBox() {

    const [stationsList, setStationsList] = useState([]);
    const [originStation, setOriginStation] = useState('');
    const [destinationStation, setDestinationStation] = useState('');
    const [departureTime, setDepartureTime] = useState(''); // Add a state for the time
    const [userLocation, setUserLocation] = useState({ latitude: null, longitude: null });  
    const [isLoading, setIsLoading] = useState(false);
    const [formattedRideTime, setFormattedRideTime] = useState('');
    const [findingCarbox, setFindingCarbox] = useState(false);
    
    const navigate = useNavigate();

    useEffect(() => {
      // get the station
      fetch("https://localhost:7158/api/stations", {
        method: "GET", 
        headers: {
          'Content-Type': 'application/json'
        }})
      .then((response) => {
          if (!response.ok) {
            throw new Error(`Server error: ${response.status}`);
          }
          
          return response.json();
        })
        .then((data) => {
          console.log("Status updated:", data);
          setStationsList(data)
        })
        .catch((error) => {
          console.error("Fetch error:", error);
        });
      //the defualt departure time is now
      const now = new Date();
      const hours = String(now.getHours()).padStart(2, '0');
      const minutes = String(now.getMinutes()).padStart(2, '0');
      setDepartureTime(`${hours}:${minutes}`);

      if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(function (position) {
          const { latitude, longitude } = position.coords;
          setUserLocation({ latitude: latitude, longitude: longitude });
          console.log(`Latitude: ${latitude}, Longitude: ${longitude}`);
        }, function (error) {
          console.error('Error getting location:', error);
        });
      }
      
    }, []);


    function handleClickNext () {
      setIsLoading(true);
      const now = new Date();
      
      // Extract hours and minutes from the departureTime (which is in HH:MM format)
      const [hours, minutes] = departureTime.split(":"); 

      // Update the current date with the selected hours and minutes
      now.setHours(parseInt(hours, 10), parseInt(minutes, 10), 0, 0); 

      // Convert the local time to UTC by subtracting the timezone offset
      const offset = now.getTimezoneOffset() * 60000; // Convert minutes to milliseconds
      const utcTime = new Date(now.getTime() - offset);

      // Format the date as ISO 8601 (YYYY-MM-DDTHH:MM:SSZ)
      setFormattedRideTime(utcTime.toISOString()); // This ensures the time is in UTC
    };

     useEffect(() => {
      
      if(isLoading){
        fetch("https://localhost:7158/api/RideOrders", {
            method: "POST", 
            headers: {
              'Content-Type': 'application/json'
            },
            body: JSON.stringify(
              {
                UserId: 50,
                // Origin: originStation,
                // Destination: destinationStation,
                source: {
                        "name": originStation},
                Destination: {
                  "name": destinationStation
                },
                RideTime: formattedRideTime
                      }
                    )
                  }).then((response) => {
                    if (!response.ok) {
                    //setIsLoading(false)
                    //setFindingCarbox(true) 
                      throw new Error(`Server error: ${response.status}`);
                    }
                    
                    return response.json();
                  })
                  .then((data) => {
                    setIsLoading(false)
                    setFindingCarbox(true) 
                    // set the data from the server
                    console.log("Status updated:", data);
                  })
                  .catch((error) => {
                    console.error("Fetch error:", error);
                  });
                }
                
    }, [isLoading]);

    const isFormValid = originStation !== '' && destinationStation !== ''; 

    if (!isLoading && findingCarbox){
      navigate('/FindingCarbox', {
        state: {
          originStation: originStation,
          destinationStation:destinationStation,
          departureTime:departureTime
        }
      });
    }

    return (
      
      
      <div >
      {!isLoading && (
        <div id='search-box'>
      <TextField
          className="search-field"
          id="outlined-select-currency-native"
          select
          label="Select your origin"
          value={originStation} // Set value to originStation state
          onChange={(e) => setOriginStation(e.target.value)} // Update state on change
          slotProps={{
            select: {
              native: true,
            },
          }}
        >
          {/* Add a default option as a placeholder */}
          <option value="" disabled>
          </option>
          {stationsList.map((station) => (
            <option key={station.name} value={station.name}>
              {station.name}
            </option>
          ))}
        </TextField>

        <TextField
          className="search-field"
          id="outlined-select-currency-native"
          select
          label="Select your destination"
          value={destinationStation} // Set value to originStation state
          onChange={(e) => setDestinationStation(e.target.value)} // Update state on change
          slotProps={{
            select: {
              native: true,
            },
          }}
        >
          {/* Add a default option as a placeholder */}
          <option value="" disabled>
          </option>
          {stationsList.map((station) => (
            <option key={station.name} value={station.name}>
              {station.name}
            </option>
          ))}
        </TextField>
        <TextField
          className="search-field"
          id="outlined-time"
          type="time"
          label="Select departure time"
          value={departureTime}
          onChange={(e) => setDepartureTime(e.target.value)}
          InputLabelProps={{
            shrink: true, // Ensures the label stays above the input
          }}
          inputProps={{
            step: 300, // Adjust step for time format (in seconds)
          }}
        />
        <Box sx={{ flex: '1 1 auto' }} />
        <Button onClick={handleClickNext}
        sx={{ mr: 1 }}
        disabled={!isFormValid} // Disable the button if form is invalid
      >
        Next
      </Button>
      <div><Map stations={stationsList} userLocation={userLocation}/></div>
      </div>
    )}
      {isLoading && (
        <div className="loading-container">
          <LoadingPage />
        </div>
      )}        
      </div>
    );
    
  }

  export default SearchBox; 