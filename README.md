<div style="margin-bottom: 20px;"> <img src="https://github.com/user-attachments/assets/bb290fe1-8a6c-40e5-92cd-b3e6646edf6c" alt="Gradia Logo" height="32" style="margin-bottom: 10px; vertical-align: 10px;"> <br> </div>

The **Gradia Backend API** powers the Gradia personalized learning platform, acting as the central hub for user management, chat interactions, document processing, and communication with the AI text style analysis engine. It ensures a seamless and secure experience for students seeking to improve their academic writing.

## ✨ Key Features

*   👤 **Comprehensive User Management**: Secure registration, login, and management of user profiles and preferences.
*   💬 **Robust Chat & Conversation Handling**: Stores user messages, maintains conversation context across sessions, and facilitates interaction with the AI.
*   📄 **Document Ingestion & Processing**: Allows users to upload documents (e.g., essays, papers). The backend extracts text content for analysis by the AI model.
*   🤖 **AI Model Orchestration**: Manages communication with the dedicated Python AI service (leveraging GPT-2 and other models) to:
    *   Send user text and uploaded document content for style determination.
    *   Receive AI-generated feedback, style analysis, and improvement suggestions.
    *   Facilitate the generation of responses in the user's preferred writing style.
*   💾 **Data Persistence & Integrity**: Utilizes SQL Server to reliably store user credentials, chat histories, conversation context, references to Ground Truth (GT) styles, and user-uploaded content.
*   🛡️ **Secure & Scalable Architecture**: Built with ASP.NET Core, ensuring a robust, secure, and scalable foundation capable of handling numerous concurrent users and interactions.
*   ⚙️ **Standardized API Endpoints**: Provides a well-defined set of API endpoints for the frontend (React with TypeScript) to interact with, ensuring clear separation of concerns.

## 🏗️ System Architecture Role

The Gradia Backend API serves as the critical middleware in the Gradia ecosystem. It sits between the user-facing frontend application and the specialized AI model service. Its primary responsibilities include:
1.  Authenticating and authorizing users.
2.  Managing user-specific data and chat sessions.
3.  Preprocessing and relaying user input/documents to the AI model.
4.  Receiving and structuring AI model responses for the frontend.
5.  Ensuring data integrity and security.

### Class Diagram

The following class diagram illustrates the core entities and their relationships within the Gradia backend data model. It highlights how user information, chat sessions, messages, and AI-driven style analyses are structured and interconnected.
![image](https://github.com/user-attachments/assets/0f336679-ceda-4197-9a9c-331886821113)

## 🔌 Gradia API v1 (Backend Core)

The API exposes functionalities for user account management, chat interactions, and document analysis.

#### 🧑 AppUser (Account)

- `POST /api/account/register`  
  Registers a new user account.

- `POST /api/account/login`  
  Authenticates an existing user and provides an access token.

#### 💬 Chat & Analysis

- `GET /api/chat`  
  Retrieves a list of the current user's chat sessions or conversation history.

- `POST /api/chat/chat`  
  Initiates a new chat session for the user.
  *(Could also be `POST /api/chat` depending on convention)*

- `GET /api/chat/{chatId}`  
  Retrieves messages and context for a specific chat session.

- `POST /api/chat/{chatId}`  
  Sends a new user message within an existing chat session to be processed by the AI.

- `DELETE /api/chat/{chatId}`  
  Deletes a specific chat session and its associated data.

- `POST /api/chat/paper-analysis`  
  Accepts an uploaded document (e.g., via multipart/form-data), processes it, and sends its content to the AI model for style analysis and feedback.

## 💻 Technologies Used (Backend Core)

*   **Framework**: ASP.NET Core (C#)
*   **Database**: SQL Server
*   **Authentication**: JWT and ASP.NET Core Identity
*   **AI Communication**: HTTP client calls to the Python AI service.
