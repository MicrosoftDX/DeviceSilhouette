# Security

## Device Security

## Service Fabric Security

Adminstrative access to the Service Fabric cluster is managed via Azure AD.

The Service Fabric cluster needs to be created with Azure AD configured,
currently Azure AD cannot be added to an already deployed cluster.

[Create Service Fabric Cluster with Azure AD](https://azure.microsoft.com/en-us/documentation/articles/service-fabric-cluster-creation-via-arm/)

### Configure User Access

To enable users access to the service fabric explorer,
[roles muse be assigned to the users](https://azure.microsoft.com/en-us/documentation/articles/service-fabric-cluster-creation-via-arm/#_assign-users-to-roles).
