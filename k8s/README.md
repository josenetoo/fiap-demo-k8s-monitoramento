# 📁 Manifests Kubernetes - FIAP Observabilidade

## 📋 Estrutura dos Arquivos

### **📱 Aplicação (.NET Weather API)**
```
k8s/app/
├── deployment.yaml      # Deploy da aplicação .NET
└── service.yaml         # Service para expor a aplicação
```

### **📊 Monitoramento**
```
k8s/monitoring/
├── servicemonitor.yaml  # ServiceMonitor para Prometheus descobrir a app
├── prometheus-rules.yaml # Alertas customizados
├── hpa.yaml            # Horizontal Pod Autoscaler
├── loki.yaml           # Loki para logs
├── promtail.yaml       # Promtail para coletar logs
└── tempo.yaml          # Tempo para traces
```

## 🚀 **Ordem de Deploy**

### **1. Aplicação**
```bash
# Deploy da aplicação .NET
kubectl apply -f k8s/app/deployment.yaml
kubectl apply -f k8s/app/service.yaml

# Verificar
kubectl get pods -l app=weather-api
kubectl get svc weather-api
```

### **2. Monitoramento da Aplicação**
```bash
# ServiceMonitor para Prometheus descobrir
kubectl apply -f k8s/monitoring/servicemonitor.yaml

# Alertas customizados
kubectl apply -f k8s/monitoring/prometheus-rules.yaml

# HPA para auto-scaling
kubectl apply -f k8s/monitoring/hpa.yaml

# Verificar
kubectl get servicemonitor
kubectl get prometheusrule -n monitoring
kubectl get hpa
```

### **3. Loki (Logs)**
```bash
# Deploy Loki
kubectl apply -f k8s/monitoring/loki.yaml

# Deploy Promtail (coleta logs)
kubectl apply -f k8s/monitoring/promtail.yaml

# Verificar
kubectl get pods -n monitoring | grep loki
kubectl get pods -n monitoring | grep promtail
```

### **4. Tempo (Traces)**
```bash
# Deploy Tempo
kubectl apply -f k8s/monitoring/tempo.yaml

# Verificar
kubectl get pods -n monitoring | grep tempo
kubectl get svc -n monitoring | grep tempo
```

## 🧪 **Testar Tudo**

### **Port-forwards**
```bash
# Grafana
kubectl port-forward svc/prometheus-grafana 3000:80 -n monitoring &

# Prometheus  
kubectl port-forward svc/prometheus-kube-prometheus-prometheus 9090:9090 -n monitoring &

# Aplicação
kubectl port-forward svc/weather-api 8080:80 &
```

### **URLs de Acesso**
- **Grafana**: http://localhost:3000 (admin/fiap2025)
- **Prometheus**: http://localhost:9090
- **Aplicação**: http://localhost:8080
- **Health Check**: http://localhost:8080/health
- **Métricas**: http://localhost:8080/metrics

### **Configurar Datasources no Grafana**
```bash
# Loki
URL: http://loki:3100

# Tempo  
URL: http://tempo:3200
```

## 🧹 **Limpeza**

### **Deletar Aplicação**
```bash
kubectl delete -f k8s/app/
kubectl delete -f k8s/monitoring/servicemonitor.yaml
kubectl delete -f k8s/monitoring/prometheus-rules.yaml
kubectl delete -f k8s/monitoring/hpa.yaml
```

### **Deletar Observabilidade**
```bash
kubectl delete -f k8s/monitoring/loki.yaml
kubectl delete -f k8s/monitoring/promtail.yaml
kubectl delete -f k8s/monitoring/tempo.yaml
```

---

**🎯 Todos os manifests prontos para o hands-on!**
