# 🏠 Manifests Kubernetes - Ambiente Local

Manifests otimizados para **Docker Desktop + Kubernetes** local.

## 📁 **Estrutura**

```
k8s-local/
├── README.md                    # Este arquivo
├── app/                         # Aplicação .NET
│   ├── deployment-local.yaml    # Deployment otimizado para local
│   └── service-local.yaml       # Service com NodePort opcional
└── monitoring/                  # Stack de observabilidade
    ├── loki-local.yaml         # Loki com storage local
    ├── promtail-local.yaml     # Promtail para Docker Desktop
    └── tempo-local.yaml        # Tempo com storage local
```

## 🚀 **Deploy Manual (Passo a Passo)**

### **1. Aplicação**
```bash
# Deploy da aplicação .NET
kubectl apply -f k8s-local/app/deployment-local.yaml
kubectl apply -f k8s-local/app/service-local.yaml

# Verificar
kubectl get pods -l app=weather-api
kubectl get svc weather-api
```

### **2. Monitoramento (após instalar Prometheus via Helm)**
```bash
# Deploy dos componentes de observabilidade
kubectl apply -f k8s-local/monitoring/loki-local.yaml
kubectl apply -f k8s-local/monitoring/promtail-local.yaml
kubectl apply -f k8s-local/monitoring/tempo-local.yaml

# Aplicar ServiceMonitor e alertas (usar originais)
kubectl apply -f k8s/monitoring/servicemonitor.yaml
kubectl apply -f k8s/monitoring/prometheus-rules.yaml
kubectl apply -f k8s/monitoring/hpa.yaml

# Verificar
kubectl get pods -n monitoring
```

### **3. Port-forwards**
```bash
# Grafana
kubectl port-forward svc/prometheus-grafana 3000:80 -n monitoring &

# Prometheus
kubectl port-forward svc/prometheus-kube-prometheus-prometheus 9090:9090 -n monitoring &

# Aplicação
kubectl port-forward svc/weather-api 8080:80 &
```

## 🔧 **Diferenças vs Ambiente AWS**

| Componente | AWS (k8s/) | Local (k8s-local/) |
|------------|------------|-------------------|
| **Deployment** | imagePullPolicy: Never | imagePullPolicy: IfNotPresent |
| **Service** | ClusterIP | NodePort (opcional) |
| **Storage** | EBS volumes | hostPath/emptyDir |
| **Resources** | Limits rígidos | Limits flexíveis |
| **Replicas** | 2 (multi-node) | 1-2 (single-node) |
| **Node-Exporter** | Habilitado | Desabilitado (incompatível) |

## 📊 **Recursos Recomendados**

```yaml
# Docker Desktop Settings
Resources:
  CPUs: 4+
  Memory: 8GB+
  Swap: 2GB
  Disk: 20GB+
```

## 🧹 **Limpeza**

```bash
# Parar port-forwards
pkill -f "kubectl port-forward"

# Deletar aplicação
kubectl delete -f k8s-local/app/

# Deletar monitoramento
kubectl delete -f k8s-local/monitoring/
kubectl delete -f k8s/monitoring/servicemonitor.yaml
kubectl delete -f k8s/monitoring/prometheus-rules.yaml
kubectl delete -f k8s/monitoring/hpa.yaml

# Desinstalar Prometheus
helm uninstall prometheus -n monitoring

# Deletar namespace
kubectl delete namespace monitoring
```

---

**🎯 Manifests otimizados para desenvolvimento local!**
