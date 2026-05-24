import { defineStore } from 'pinia';

export interface CartItem {
  id: number;
  name: string;
  price: number;
  image: string | null;
  quantity: number;
  saleUnitId: number | null;
}

export const useCartStore = defineStore('cart', {
  state: () => ({
    items: [] as CartItem[]
  }),
  getters: {
    totalItems: (state) => state.items.reduce((acc, item) => acc + item.quantity, 0),
    totalAmount: (state) => state.items.reduce((acc, item) => acc + (item.price * item.quantity), 0)
  },
  actions: {
    addToCart(product: any) {
      const existing = this.items.find(item => item.id === product.id);
      if (existing) {
        existing.quantity += 1;
      } else {
        this.items.push({
          id: product.id,
          name: product.name,
          price: product.price || 0,
          image: product.image || null,
          quantity: 1,
          saleUnitId: product.saleUnitId || null
        });
      }
    },
    updateQuantity(productId: number, quantity: number) {
      const item = this.items.find(item => item.id === productId);
      if (item) {
        item.quantity = quantity;
        if (item.quantity <= 0) {
          this.removeFromCart(productId);
        }
      }
    },
    removeFromCart(productId: number) {
      this.items = this.items.filter(item => item.id !== productId);
    },
    clearCart() {
      this.items = [];
    }
  }
});
