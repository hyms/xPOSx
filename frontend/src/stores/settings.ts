import { defineStore } from 'pinia';
import { api } from 'boot/axios';

export interface SettingState {
  id: number;
  companyName: string;
  email: string | null;
  companyPhone: string | null;
  companyAddress: string | null;
  logo: string | null;
  favicon: string | null;
  version: string;
  settingsVersion: number;
  currencyCode: string | null;
  currencySymbol: string | null;
}

export const useSettingsStore = defineStore('settings', {
  state: () => ({
    settings: null as SettingState | null,
    loading: false,
  }),

  getters: {
    getLogoUrl: (state) => {
      if (!state.settings?.logo) return '/icons/favicon-128x128.png';
      return state.settings.logo.startsWith('http') 
        ? state.settings.logo 
        : `${process.env.VITE_API_URL || ''}${state.settings.logo}`;
    },
    getFaviconUrl: (state) => {
      if (!state.settings?.favicon) return '/favicon.ico';
      return state.settings.favicon.startsWith('http') 
        ? state.settings.favicon 
        : `${process.env.VITE_API_URL || ''}${state.settings.favicon}`;
    }
  },

  actions: {
    async fetchSettings() {
      this.loading = true;
      try {
        const response = await api.get<SettingState>('/settings');
        this.settings = response.data;
        if (this.settings && this.settings.settingsVersion) {
          localStorage.setItem('settings_version', String(this.settings.settingsVersion));
        }
        this.updateFavicon();
      } catch (error) {
        console.error('Error fetching settings:', error);
      } finally {
        this.loading = false;
      }
    },

    updateSettingsVersion(version: number) {
      if (this.settings && this.settings.settingsVersion < version) {
        this.fetchSettings();
      }
    },

    updateFavicon() {
      if (!this.settings?.favicon) return;
      const links: NodeListOf<HTMLLinkElement> = document.querySelectorAll("link[rel*='icon']");
      const url = this.getFaviconUrl;
      const version = this.settings.settingsVersion || Date.now();
      
      if (links.length > 0) {
        links.forEach(link => {
          link.href = `${url}?v=${version}`;
        });
      } else {
        const newLink = document.createElement('link');
        newLink.rel = 'icon';
        newLink.href = `${url}?v=${version}`;
        document.head.appendChild(newLink);
      }
    }
  }
});
