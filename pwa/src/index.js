import React from 'react';
import './index.css';
import ReactDOM from 'react-dom/client';
import { Routes, Route, BrowserRouter } from "react-router-dom";
import App from './App';
import SearchBox from './SearchBox';
import LoadingPage from './LoadingPage';
import FindingCarbox from './FindingCarbox';
import CarboxArrived from './CarboxArrived';
import QRScanner from './QRScanner';
import * as serviceWorkerRegistration from './serviceWorkerRegistration';
import reportWebVitals from './reportWebVitals';

const root = ReactDOM.createRoot(document.getElementById('root'));
root.render(
  <BrowserRouter>
     <Routes>
      <Route path="/" element={<SearchBox />} />
      <Route path="/LoadingPage" element={<LoadingPage />} />
      <Route path="/FindingCarbox" element={<FindingCarbox />} />
      <Route path="/CarboxArrived" element={<CarboxArrived />} />
      <Route path="/QRScanner" element={<QRScanner />} />
    </Routes>
</BrowserRouter>
);

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://cra.link/PWA
serviceWorkerRegistration.register();

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
