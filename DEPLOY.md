# CARBOX Backend Deployment Guide for Render

## Prerequisites
- A Render account (free tier available)
- Your MongoDB connection string
- This repository with the Dockerfile

## Deployment Steps

1. **Prepare your repository:**
   - Ensure the Dockerfile and .dockerignore are in your project root
   - Commit all changes to your Git repository
   - Push to GitHub, GitLab, or Bitbucket

2. **Create a new Web Service on Render:**
   - Go to [Render Dashboard](https://dashboard.render.com)
   - Click "New +" and select "Web Service"
   - Connect your Git repository

3. **Configure the service:**
   - **Name**: `carbox-backend` (or your preferred name)
   - **Runtime**: Docker
   - **Build Command**: (leave empty - Docker handles this)
   - **Start Command**: (leave empty - Docker handles this)
   - **Plan**: Free tier is sufficient for testing

4. **Set Environment Variables:**
   In the Environment section, add:
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ConnectionStrings__DbConnection=mongodb+srv://your-username:your-password@your-cluster...
   ```

5. **Deploy:**
   - Click "Create Web Service"
   - Render will automatically build and deploy your Docker container
   - Wait for the deployment to complete (usually 2-5 minutes)

## Testing Your Deployment

Once deployed, you can:

1. **Access Swagger UI**: Visit your Render URL (e.g., `https://your-service-name.onrender.com`)
   - Swagger UI will be available at the root URL
   - Test all your API endpoints directly in the browser

2. **Test API endpoints**: Use the base URL `https://your-service-name.onrender.com/api/`

## Important Notes

- **Database Connection**: Make sure your MongoDB connection string is properly configured in environment variables
- **CORS**: The application is configured to allow all origins for flexibility
- **SSL**: Render handles SSL termination automatically
- **Logs**: Check deployment logs in Render dashboard if there are issues
- **Free Tier**: Render free tier may sleep after 15 minutes of inactivity

## Troubleshooting

If deployment fails:
1. Check the build logs in Render dashboard
2. Verify your MongoDB connection string is correct
3. Ensure all NuGet packages are properly referenced in the .csproj file
4. Check that the port 8080 is correctly exposed in the Dockerfile