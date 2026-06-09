pipeline {
    agent any

    triggers {
        // Trigger when a push is made to GitHub
        githubPush()
    }

    options {
        timeout(time: 1, unit: 'HOURS')
        buildDiscarder(logRotator(numToKeepStr: '10'))
        disableConcurrentBuilds()
    }

    stages {
        stage('Detect Changes') {
            steps {
                script {
                    echo "Changes detected in fifa-backend repository. Starting build process..."
                }
            }
        }

        stage('Restore NuGet Packages') {
            steps {
                echo 'Restoring .NET packages...'
                bat 'dotnet restore'
            }
        }

        stage('Compile Backend') {
            steps {
                echo 'Compiling the solution...'
                bat 'dotnet build --configuration Release --no-restore'
            }
        }

        stage('Run Unit & Integration Tests') {
            steps {
                echo 'Running tests...'
                bat 'dotnet test --configuration Release --no-build --verbosity normal'
            }
        }

        stage('Database Integration Validate') {
            steps {
                echo 'Spinning up MySQL DB test instance...'
                // Start only the database service to run migrations checks
                bat 'docker compose down --remove-orphans || exit /b 0'
                bat 'docker compose up -d db'
                
                echo 'Waiting for MySQL database to initialize...'
                // Wait for MySQL to start up and listen using Windows ping delay (approx 15 seconds)
                bat 'ping 127.0.0.1 -n 16 > nul'
            }
        }

        stage('Docker Package') {
            steps {
                echo 'Building backend Docker image...'
                bat 'docker build -t fifa-backend:latest .'
            }
        }
    }

    post {
        always {
            echo 'Cleaning up docker containers...'
            bat 'docker compose down || exit /b 0'
        }
    }
}
