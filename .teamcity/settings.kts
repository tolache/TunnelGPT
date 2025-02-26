import buildTypes.*
import jetbrains.buildServer.configs.kotlin.*

version = "2024.12"

project {
    buildType(GenerateCert)
    buildType(Build)
    buildType(Deploy)

    buildTypesOrder = arrayListOf(GenerateCert, Build, Deploy)

    params {
        text("branch_name", "hosted", readOnly = true, allowEmpty = false)
        password("target_servername", "credentialsJSON:184ea855-6e00-41af-ba17-ae8170abc550")
    }
}