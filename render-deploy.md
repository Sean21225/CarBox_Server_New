# CARBOX Backend - Render Deployment Guide

## Quick Deployment Steps

### 1. GitHub Setup
```bash
# Initialize git (if not already done)
git init
git add .
git commit -m "Initial CARBOX backend for Render deployment"
git branch -M main
git remote add origin https://github.com/YOUR_USERNAME/carbox-backend.git
git push -u origin main
```

### 2. Render Configuration

1. **Create New Web Service**:
   - Go to [Render Dashboard](https://dashboard.render.com)
   - Click "New" → "Web Service"
   - Connect your GitHub repository

2. **Service Settings**:
   - **Name**: `carbox-backend`
   - **Environment**: `Docker`
   - **Build Command**: Leave empty
   - **Start Command**: Leave empty
   - **Dockerfile Path**: `Dockerfile`

3. **Environment Variables** (Required):
   ```
   MONGODB_CONNECTION_STRING=mongodb+srv://username:password@cluster.mongodb.net/carbox
   DATABASE_NAME=carbox
   ```

4. **Optional Environment Variables**:
   ```
   MQTT_SERVER=your-mqtt-broker.com
   MQTT_PORT=1883
   MQTT_USERNAME=your_username
   MQTT_PASSWORD=your_password
   ```

### 3. Database Setup (MongoDB Atlas)

1. **Create MongoDB Atlas Cluster**:
   - Go to [MongoDB Atlas](https://cloud.mongodb.com)
   - Create a free cluster
   - Set up database user and password
   - Whitelist all IP addresses (0.0.0.0/0) for Render

2. **Get Connection String**:
   - Click "Connect" → "Connect your application"
   - Copy the connection string
   - Replace `<password>` with your database password
   - Use this as `MONGODB_CONNECTION_STRING`

### 4. Deploy

1. Click "Create Web Service" in Render
2. Render will automatically:
   - Pull your code from GitHub
   - Build the Docker container
   - Deploy your API
   - Provide a public URL

### 5. Test Deployment

Your API will be available at: `https://your-app-name.onrender.com`

Test endpoints:
- Health check: `GET https://your-app-name.onrender.com/health`
- API docs: `https://your-app-name.onrender.com/swagger`
- Stations: `GET https://your-app-name.onrender.com/api/stations`

### 6. Update Frontend

Update your React PWA to use the new API URL instead of localhost:

```javascript
// Replace localhost URLs with your Render URL
const API_BASE_URL = "https://your-app-name.onrender.com";

fetch(`${API_BASE_URL}/api/stations`)
```

## Troubleshooting

- **Build fails**: Check Dockerfile syntax and .NET compatibility
- **Connection errors**: Verify MongoDB connection string and whitelist IPs
- **CORS issues**: Backend is configured to allow all origins
- **Health check fails**: Check `/health` endpoint responds correctly

## Free Tier Notes

- Render free tier sleeps after 15 minutes of inactivity
- First request after sleep may take 30+ seconds
- Consider upgrading to paid tier for production use