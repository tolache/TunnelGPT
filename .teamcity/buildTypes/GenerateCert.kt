package buildTypes

import java.io.File
import jetbrains.buildServer.configs.kotlin.*
import jetbrains.buildServer.configs.kotlin.buildSteps.PowerShellStep
import jetbrains.buildServer.configs.kotlin.buildSteps.powerShell
import org.intellij.lang.annotations.Language

@Language("file-path")
val scriptContent = File("./buildScripts/generate-cert.ps1").readText()

class GenerateCert : BuildType({
    id("GenerateCertificate")
    name = "Generate Certificate"

    artifactRules = "tunnelgpt-cert.* => tunnelgpt-cert.zip"

    vcs {
        cleanCheckout = true
    }

    steps {
        powerShell {
            name = "Create certificate"
            id = "Create_certificate"
            edition = PowerShellStep.Edition.Core
            formatStderrAsError = true
            scriptMode = script {
                content = scriptContent
            }
            scriptArgs = "-Servername %target_servername%"
        }
    }
})