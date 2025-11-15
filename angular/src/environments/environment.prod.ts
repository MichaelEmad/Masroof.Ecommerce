import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

const oAuthConfig = {
  issuer: 'https://localhost:44302/',
  redirectUri: baseUrl,
  clientId: 'Ecommerce_App',
  responseType: 'code',
  scope: 'offline_access Ecommerce',
  requireHttps: true,
  impersonation: {
    userImpersonation: true,
  }
};

export const environment = {
  production: true,
  application: {
    baseUrl,
    name: 'Ecommerce',
  },
  oAuthConfig,
  apis: {
    default: {
      url: 'https://localhost:44302',
      rootNamespace: 'Masroof.Ecommerce',
    },
    AbpAccountPublic: {
      url: oAuthConfig.issuer,
      rootNamespace: 'AbpAccountPublic',
    },
  },
  stripe: {
    publishableKey: 'pk_live_YOUR_LIVE_PUBLISHABLE_KEY_HERE',
  },
  remoteEnv: {
    url: '/getEnvConfig',
    mergeStrategy: 'deepmerge'
  }
} as Environment;
