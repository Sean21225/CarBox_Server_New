
import * as React from 'react';
import { useLocation, useNavigate } from "react-router-dom";
import FadeLoader from "react-spinners/FadeLoader";
import './LoadingPage.css';

function LoadingPage() {

  return (
    
    <div>
    <div id='loading-box'>
    Looking for a CARBOX..
    <FadeLoader />
    </div>
    </div>
  );
  
}

export default LoadingPage; 


