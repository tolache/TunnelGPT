import buildTypes.*
import jetbrains.buildServer.configs.kotlin.*

version = "2025.03"

project {
    val generateCert = GenerateCert()
    buildType(generateCert)
    val buildBinaries = BuildBinaries(generateCert)
    buildType(buildBinaries)
    val deploy = Deploy(buildBinaries)
    buildType(deploy)

    buildTypesOrder = arrayListOf(generateCert, buildBinaries, deploy)

    params {
        text("branch_name", "hosted", readOnly = true, allowEmpty = false)
        password("target_servername", "credentialsJSON:184ea855-6e00-41af-ba17-ae8170abc550")
    }
}