import React, { useEffect, useRef } from 'react';
import * as atlas from 'azure-maps-control';
import 'azure-maps-control/dist/atlas.min.css';
import './AzureMap.css';

function AzureMap({ subscriptionKey, stations, userLocation }) {
  const mapRef = useRef(null);

  useEffect(() => {
    let map;

    if (mapRef.current) { // Check for mapRef here
      console.log(userLocation);
      const loadMap = () => {
        map = new atlas.Map(mapRef.current, {
          authOptions: {
            authType: atlas.AuthenticationType.subscriptionKey,
            subscriptionKey: subscriptionKey,
          },
          center: stations.length > 0 ? [userLocation.longitude, userLocation.latitude] : [0, 0],
          zoom: 14,
          view: 'Auto',
        });

        map.events.add('ready', () => {
          // Add a red marker for the user's location
          const userMarker = new atlas.HtmlMarker({
            color: 'red',
            text: 'You', // Optional, to label the user's location
            position: [userLocation.longitude, userLocation.latitude],
          });

          const userPopup = new atlas.Popup({
            pixelOffset: [0, -30],
          });

          userPopup.setOptions({
            position: [userLocation.longitude, userLocation.latitude],
            content: `<div style="padding:10px;">Your Location</div>`,
          });

          // Attach the popup to the user's marker
          map.markers.add(userMarker);
          map.popups.add(userPopup);

          // Add markers for the stations
          stations.forEach((element) => {
            const marker = new atlas.HtmlMarker({
              color: 'DodgerBlue',
              text: element.name,
              position: [element.location.longitude, element.location.latitude],
            });

            const popup = new atlas.Popup({
              pixelOffset: [0, -30],
            });

            map.events.add('click', marker, () => {
              popup.setOptions({
                position: [element.location.longitude, element.location.latitude],
                content: `<div style="padding:10px;">${element.name}<br/>${element.address}</div>`,
              });

              popup.open(map);
            });

            map.markers.add(marker);
            map.popups.add(popup);
          });
        });
      };

      loadMap();

      return () => {
        if (map) {
          map.dispose(); // Cleanup map instance on component unmount or dependency change
        }
      };
    }
  }, [subscriptionKey, stations, userLocation]); // Added userLocation to dependencies

  return <div ref={mapRef} id="map" className='w-full'></div>;
}

export default AzureMap;
