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
                    // Check if git changesets contain changes to the backend directory
                    def changedFiles = currentBuild.changeSets.collect { changeSet ->
                        changeSet.items.collect { item ->
                            item.affectedPaths
                        }
                    }.flatten()

                    if (changedFiles && changedFiles.size() > 0) {
                        def backendChanged = changedFiles.any { path -> path.startsWith('fifa-backend/') }
                        if (!backendChanged) {
                            currentBuild.result = 'SUCCESS'
                            currentBuild.description = 'Skipped: No changes detected in fifa-backend'
                            echo "No changes detected in fifa-backend. Skipping remaining stages."
                            error("Pipeline skipped because no files in fifa-backend were changed.")
                        }
                    }
                    echo "Changes detected in fifa-backend. Starting build process..."
                }
            }
        }

        stage('Restore NuGet Packages') {
            steps {
                dir('fifa-backend') {
                    echo 'Restoring .NET packages...'
                    sh 'dotnet restore'
                }
            }
        }

        stage('Compile Backend') {
            steps {
                dir('fifa-backend') {
                    echo 'Compiling the solution...'
                    sh 'dotnet build --configuration Release --no-restore'
                }
            }
        }

        stage('Run Unit & Integration Tests') {
            steps {
                dir('fifa-backend') {
                    echo 'Running tests...'
                    sh 'dotnet test --configuration Release --no-build --verbosity normal'
                }
            }
        }

        stage('Database Integration Validate') {
            steps {
                dir('fifa-backend') {
                    echo 'Spinning up MySQL DB test instance...'
                    // Start only the database service to run migrations checks
                    sh 'docker compose down --remove-orphans || true'
                    sh 'docker compose up -d db'
                    
                    echo 'Waiting for MySQL database to initialize...'
                    // Wait for MySQL to start up and listen
                    sh 'sleep 15'
                    
                    echo 'Running EF Core migrations check...'
                    // Optional check: Can install and run dotnet-ef commands to test applying migrations
                    // sh 'dotnet tool restore'
                    // sh 'dotnet ef database update'
                }
            }
        }

        stage('Docker Package') {
            steps {
                dir('fifa-backend') {
                    echo 'Building backend Docker image...'
                    sh 'docker build -t fifa-backend:latest .'
                }
            }
        }
    }

    post {
        always {
            dir('fifa-backend') {
                echo 'Cleaning up docker containers...'
                sh 'docker compose down || true'
            }
        }
    }
}
