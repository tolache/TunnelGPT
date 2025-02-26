package buildTypes

import jetbrains.buildServer.configs.kotlin.*
import jetbrains.buildServer.configs.kotlin.buildFeatures.commitStatusPublisher
import jetbrains.buildServer.configs.kotlin.buildSteps.PowerShellStep
import jetbrains.buildServer.configs.kotlin.buildSteps.dotnetBuild
import jetbrains.buildServer.configs.kotlin.buildSteps.dotnetTest
import jetbrains.buildServer.configs.kotlin.buildSteps.powerShell

object Build : BuildType({
    id("Build")
    name = "Build"

    artifactRules = "publish/** => TunnelGPT_build%build.number%.zip"

    params {
        password("env.OPENAI_API_KEY", "credentialsJSON:05a7b5b8-6ddb-4de4-b29e-41030e00a821")
        password("env.TELEGRAM_BOT_SECRET", "credentialsJSON:6c7c9f01-d820-4652-9580-90bae2f0d3df")
        password("tunnelgpt_bot_token","credentialsJSON:c9ff78c9-8008-4061-9e8c-6e3325f39fcc")
        password("tunnelgpt_staging_bot_token","credentialsJSON:1ad33c17-c3e1-45c9-baca-56e1774bc651")
        text("env.TELEGRAM_BOT_TOKEN", "%tunnelgpt_staging_bot_token%", allowEmpty = false)
    }

    vcs {
        root(DslContext.settingsRoot)
    }

    steps {
        powerShell {
            name = "Initialize appsettings.json"
            id = "Initialize_appsettings_json"
            platform = PowerShellStep.Platform.x64
            edition = PowerShellStep.Edition.Core
            formatStderrAsError = true
            scriptMode = file {
                path = ".teamcity/buildScripts/init-appsettings.ps1"
            }
        }
        dotnetTest {
            name = "Test"
            id = "Test"
            args = "-p:ContinuousIntegrationBuild=true"
            coverage = dotcover {
                assemblyFilters = "-:*Tests"
            }
        }
        dotnetBuild {
            name = "Publish"
            id = "Publish"
            projects = "TunnelGPT"
            configuration = "Release"
            runtime = "linux-x64"
            outputDir = "publish"
            args = """
                --self-contained false -p:ContinuousIntegrationBuild=true
                -p:Version=1.0.0
                -p:FileVersion=1.0.0.%build.number%
                -p:AssemblyVersion=1.0.0.%build.number%
            """.trimIndent()
        }
    }

    features {
        commitStatusPublisher {
            vcsRootExtId = "${DslContext.settingsRoot.id}"
            publisher = github {
                githubUrl = "https://api.github.com"
                authType = vcsRoot()
            }
        }
    }

    dependencies {
        dependency(GenerateCert) {
            snapshot {
                onDependencyFailure = FailureAction.FAIL_TO_START
            }
            artifacts {
                artifactRules = "tunnelgpt-cert.zip!* => publish"
            }
        }
    }
})