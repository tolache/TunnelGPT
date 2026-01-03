{{- define "tunnelgpt.selectorLabels" -}}
app: {{ .Release.Name }}
{{- end }}

{{- define "tunnelgpt.labels" -}}
{{ include "tunnelgpt.selectorLabels" . }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}
