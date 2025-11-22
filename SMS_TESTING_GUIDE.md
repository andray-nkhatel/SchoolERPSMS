# SMS Integration Testing Guide

This guide explains how to test the Zamtel SMS integration using Swagger UI.

## Prerequisites

1. **Get your Zamtel API Key** from your Zamtel Bulk SMS account
2. **Configure the API key** (see Configuration section below)
3. **Have a valid JWT token** for authentication (Admin or Staff role)

## Configuration

### Option 1: Using appsettings.json (Development Only)

Edit `appsettings.json`:

```json
{
  "Sms": {
    "BaseUrl": "https://bulksms.zamtel.co.zm/api/v2.1/action/",
    "ApiKey": "your-actual-api-key-here",
    "SenderId": "Zamtel"
  }
}
```

### Option 2: Using Environment Variables (Recommended)

Set the environment variable:

```bash
export Sms__ApiKey="your-actual-api-key-here"
export Sms__SenderId="Zamtel"
```

Or add to your `.env` file (if using one):

```
Sms__ApiKey=your-actual-api-key-here
Sms__SenderId=Zamtel
```

## Testing with Swagger UI

### Step 1: Start the Application

```bash
dotnet run
```

The application will start and display the URL where it's running (typically `http://localhost:5000` or `https://localhost:5001`).

### Step 2: Open Swagger UI

1. Navigate to the Swagger UI URL (usually `http://localhost:5000` or `https://localhost:5001`)
2. Swagger UI will display all available API endpoints

### Step 3: Authenticate

Before testing SMS endpoints, you need to authenticate:

1. **Find the `POST /api/auth/login` endpoint** in Swagger UI
2. Click **"Try it out"**
3. Enter your credentials:
   ```json
   {
     "username": "your-username",
     "password": "your-password"
   }
   ```
4. Click **"Execute"**
5. Copy the `token` value from the response
6. Click the **"Authorize"** button (lock icon) at the top of the Swagger UI
7. In the "Value" field, enter: `Bearer YOUR_TOKEN_HERE` (replace `YOUR_TOKEN_HERE` with the actual token)
8. Click **"Authorize"** and then **"Close"**

Now you're authenticated and can test the SMS endpoints!

### Step 4: Test Single SMS

1. **Find the `POST /api/sms/send` endpoint** in Swagger UI
2. Click **"Try it out"**
3. The request body will be pre-filled with an example. Modify it:
   ```json
   {
     "phoneNumber": "260950003929",
     "message": "Hello! This is a test message from the School Management System."
   }
   ```
4. Click **"Execute"**
5. Check the response:
   - **200 OK**: SMS sent successfully
   - **400 Bad Request**: Missing phone number or message
   - **401 Unauthorized**: Invalid or missing token
   - **403 Forbidden**: Insufficient permissions (requires Admin or Staff role)
   - **500 Internal Server Error**: SMS service error

### Step 5: Test Bulk SMS

1. **Find the `POST /api/sms/send/bulk` endpoint** in Swagger UI
2. Click **"Try it out"**
3. Modify the request body:
   ```json
   {
     "phoneNumbers": [
       "260950003929",
       "260950003930",
       "0950003929"
     ],
     "message": "Important announcement: School will be closed tomorrow."
   }
   ```
4. Click **"Execute"**
5. Check the response (same status codes as above)

**Note:** Bulk SMS requires Admin role only.

## Phone Number Formats

The service accepts phone numbers in multiple formats and automatically formats them. In Swagger UI, you can use any of these formats:

- `260950003929` ✅ (with country code)
- `0950003929` ✅ (local format, will automatically add country code 260)
- `950003929` ✅ (local format, will automatically add country code 260)
- `+260950003929` ✅ (will remove + and use 260)

## Expected Responses in Swagger UI

### Success Response (200 OK) - Single SMS:
```json
{
  "success": true,
  "message": "SMS sent successfully",
  "phoneNumber": "260950003929"
}
```

### Success Response (200 OK) - Bulk SMS:
```json
{
  "success": true,
  "message": "All SMS messages sent successfully",
  "count": 3
}
```

### Error Responses:

**400 Bad Request** (missing phone number or message):
```json
{
  "message": "Phone number is required"
}
```

**401 Unauthorized** (missing or invalid token):
The response will indicate authentication is required.

**403 Forbidden** (insufficient permissions):
The response will indicate you don't have the required role (Admin or Staff for single SMS, Admin only for bulk SMS).

**500 Internal Server Error** (SMS service failure):
```json
{
  "message": "Failed to send SMS. Please check your SMS API configuration and logs.",
  "error": "Detailed error message (if available)"
}
```

## Checking Logs

The SMS service logs all operations. Check your console output or log files for:

- **Success logs:**
  ```
  Sending SMS to 260950003929 via Zamtel API
  SMS sent successfully to 260950003929. Response: ...
  ```

- **Error logs:**
  ```
  SMS send failed to 260950003929. Status: 400, Response: ...
  Error sending SMS to 260950003929
  ```

## Troubleshooting

### Issue: "SMS API key is not configured"
**Solution:** Make sure you've set the `Sms:ApiKey` in `appsettings.json` or as an environment variable `Sms__ApiKey`.

### Issue: "Unauthorized" or "Forbidden"
**Solution:** 
- Make sure you're using a valid JWT token
- Ensure your user has Admin or Staff role
- For bulk SMS, only Admin role is allowed

### Issue: SMS not being received
**Solution:**
- Check the API response in the logs
- Verify your Zamtel API key is valid and has credits
- Verify the phone number format is correct
- Check Zamtel account for delivery status

### Issue: Timeout errors
**Solution:**
- Check your internet connection
- Verify the Zamtel API is accessible
- Check if there are any firewall restrictions

## Testing Checklist

- [ ] API key configured correctly in `appsettings.json` or environment variables
- [ ] Application running and accessible
- [ ] Swagger UI opened in browser
- [ ] Successfully authenticated via `/api/auth/login`
- [ ] JWT token added to Swagger authorization
- [ ] Single SMS test successful (`POST /api/sms/send`)
- [ ] Bulk SMS test successful (`POST /api/sms/send/bulk`)
- [ ] Logs showing successful API calls (check console output)
- [ ] SMS received on test phone number

## Example Test Scenarios in Swagger UI

1. **Single SMS to valid number**
   - Use phone number: `260950003929`
   - Message: `"Test message"`

2. **Single SMS with local format**
   - Use phone number: `0950003929` (will auto-format to `260950003929`)
   - Message: `"Test with local format"`

3. **Bulk SMS to multiple numbers**
   - Use phone numbers: `["260950003929", "260950003930", "0950003929"]`
   - Message: `"Bulk test message"`

4. **SMS with special characters**
   - Test URL encoding with message: `"Test message with # and & symbols"`

5. **SMS with long message**
   - Test message length limits (check Zamtel API documentation for limits)

6. **Invalid request (missing phone number)**
   - Leave `phoneNumber` empty to test validation

7. **Invalid request (missing message)**
   - Leave `message` empty to test validation

## Tips for Swagger UI Testing

- **View Request/Response**: Swagger UI shows the exact request sent and response received
- **Copy cURL**: Click "Copy" button to get the cURL command for the request
- **Schema Documentation**: Click on the model schemas to see detailed field descriptions
- **Try Different Values**: Use the "Example Value" dropdown to see different example requests
- **Check Logs**: Watch your console output for detailed logging information

