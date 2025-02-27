package buildTypes

import jetbrains.buildServer.configs.kotlin.*
import jetbrains.buildServer.configs.kotlin.buildFeatures.commitStatusPublisher
import jetbrains.buildServer.configs.kotlin.buildSteps.PowerShellStep
import jetbrains.buildServer.configs.kotlin.buildSteps.dotnetBuild
import jetbrains.buildServer.configs.kotlin.buildSteps.dotnetTest
import jetbrains.buildServer.configs.kotlin.buildSteps.powerShell

class BuildBinaries(private val dependency: GenerateCert) : BuildType({
    val initAppsettingsScriptCompileTimePath = "buildScripts/init-appsettings.ps1"
    val appVersion = "1.0.0"

    id("BuildBinaries")
    name = "Build Binaries"

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
            edition = PowerShellStep.Edition.Core
            formatStderrAsError = true
            scriptMode = file {
                path = ".teamcity/$initAppsettingsScriptCompileTimePath"
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
                -p:Version=$appVersion
                -p:FileVersion=$appVersion.%build.number%
                -p:AssemblyVersion=$appVersion.%build.number%
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
        dependency(dependency) {
            snapshot {
                onDependencyFailure = FailureAction.FAIL_TO_START
            }
            artifacts {
                artifactRules = "tunnelgpt-cert.zip!* => publish"
            }
        }
    }
})