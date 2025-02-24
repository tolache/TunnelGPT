package buildTypes

import jetbrains.buildServer.configs.kotlin.*
import jetbrains.buildServer.configs.kotlin.buildSteps.PowerShellStep
import jetbrains.buildServer.configs.kotlin.buildSteps.powerShell

object GenerateCert : BuildType({
    id("GenerateCertificate")
    name = "Generate Certificate"

    artifactRules = "tunnelgpt-cert.* => tunnelgpt-cert.zip"

    steps {
        powerShell {
            name = "Create certificate"
            id = "Create_certificate"
            edition = PowerShellStep.Edition.Core
            formatStderrAsError = true
            scriptMode = file {
                path = ".teamcity/generate-cert.ps1"
            }
            scriptArgs = "-Servername %target_servername%"
        }
    }
})