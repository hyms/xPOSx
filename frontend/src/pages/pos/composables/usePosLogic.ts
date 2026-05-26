import { ref, reactive, computed } from "vue";
import { useQuasar } from "quasar";
import { useRouter } from "vue-router";
import { useCashShiftStore } from "@/stores/cashShiftStore";
import { useWarehouseStore } from "@/stores/warehouse";
import { productService } from "@/services/product.service";
import { categoryService } from "@/services/category.service";
import { warehouseService } from "@/services/warehouse.service";
import { clientService } from "@/services/client.service";
import { saleService } from "@/services/sale.service";
import {
  cashShiftService,
  type CashShift,
  type CashRegister,
} from "@/services/cashShift.service";
import { useCurrency } from "@/composables/useCurrency";
import type {
  Product,
  Category,
  Warehouse,
  Client,
  Sale,
  SaleDetail,
} from "@/types";

export function usePosLogic() {
  const $q = useQuasar();
  const router = useRouter();
  const { formatCurrency } = useCurrency();

  // State
  const search = ref("");
  const selectedCategory = ref(null);
  const categories = ref<Category[]>([]);
  const allProducts = ref<Product[]>([]);
  const warehouses = ref<Warehouse[]>([]);
  const clients = ref<Client[]>([]);
  const stocks = ref<Record<number, number>>({});

  const cart = ref<SaleDetail[]>([]);
  const showCheckoutDialog = ref(false);
  const showCartMobile = ref(false);
  const showScanner = ref(false);
  const showScanResultDialog = ref(false);
  const lastScannedProduct = ref<Product | null>(null);

  const paidAmount = ref(0);
  const change = ref(0);
  const saving = ref(false);
  const isSearchingClient = ref(false);

  const store = useCashShiftStore();
  const warehouseStore = useWarehouseStore();
  const activeShift = computed(() => store.activeShift);
  const registers = computed(() => store.registers);
  const showOpenShiftDialog = ref(false);
  const openShiftStartingCash = ref(0);
  const selectedRegisterId = ref<number | null>(null);

  const formData = reactive({
    clientId: 0,
    clientNit: "0",
    clientName: "Sin nombre",
    warehouseId: 0,
    date: new Date().toISOString().split("T")[0],
    taxRate: 0,
    discount: 0,
    shipping: 0,
    notes: "",
  });

  const SIN_LIMIT = 1000;
  const isInvoiceBlocked = computed(() => {
    // 1. Si es el cliente genérico (NIT "0" o "Sin nombre"), NO bloquear (return false)
    if (formData.clientNit === "0" || formData.clientName === "Sin nombre") {
      return false;
    }

    // 2. Si es cualquier otro cliente, aplicar la restricción del SIN (exigir NIT/CI real)
    // Se bloquea si el NIT es nulo, vacío o "0" y el total supera el límite
    return (
      (!formData.clientNit || formData.clientNit === "0") &&
      total.value > SIN_LIMIT
    );
  });

  const quickAmounts = [10, 20, 50, 100];

  // Computed
  const subtotal = computed(() =>
    cart.value.reduce((sum: number, item: SaleDetail) => sum + item.total, 0),
  );

  const total = computed(() => {
    const taxAmount = subtotal.value * ((formData.taxRate || 0) / 100);
    return (
      subtotal.value +
      taxAmount +
      (formData.shipping || 0) -
      (formData.discount || 0)
    );
  });

  const filteredProducts = computed(() => {
    return allProducts.value.filter((p: Product) => {
      const matchesSearch =
        p.name.toLowerCase().includes(search.value.toLowerCase()) ||
        (p.code && p.code.toLowerCase().includes(search.value.toLowerCase()));
      const matchesCategory =
        !selectedCategory.value || p.categoryId === selectedCategory.value;
      return matchesSearch && matchesCategory;
    });
  });

  // Methods
  const calculateChange = () => {
    change.value = paidAmount.value - total.value;
  };

  const calculateTotals = () => {
    paidAmount.value = total.value;
    calculateChange();
  };

  const setQuickAmount = (amount: number) => {
    paidAmount.value = amount;
    calculateChange();
  };

  const updateCartItem = (item: SaleDetail, quantity: number) => {
    item.quantity = quantity;
    item.total = item.quantity * item.price;
    calculateTotals();
  };

  const addToCart = (product: Product) => {
    const existing = cart.value.find(
      (item: SaleDetail) => item.productId === product.id,
    );
    if (existing) {
      updateCartItem(existing, existing.quantity + 1);
    } else {
      cart.value.push({
        productId: product.id!,
        quantity: 1,
        price: product.price,
        total: product.price,
      });
      calculateTotals();
    }
  };

  const handleBarcodeScan = () => {
    const code = search.value.trim();
    if (!code) return;

    const exactMatch = allProducts.value.find(
      (p) => p.code?.trim() === code || p.id?.toString() === code,
    );

    if (exactMatch) {
      addToCart(exactMatch);
      search.value = "";
      lastScannedProduct.value = exactMatch;

      if (showScanner.value) {
        showScanResultDialog.value = true;
      } else {
        $q.notify({
          color: "positive",
          message: `<strong>${exactMatch.name}</strong> añadido al carrito`,
          caption: `Precio: ${formatCurrency(exactMatch.price)}`,
          icon: "shopping_cart",
          html: true,
          timeout: 1000,
          position: "center",
        });
      }
    } else {
      $q.notify({
        color: "warning",
        message: `Código "${code}" no encontrado`,
        icon: "error_outline",
        timeout: 2000,
      });
    }
  };

  const getProductName = (id: number) =>
    allProducts.value.find((p: Product) => p.id === id)?.name || "";

  const getStock = (productId: number) => stocks.value[productId] || 0;

  const fetchStocks = async () => {
    if (!formData.warehouseId) return;
    try {
      // Placeholder logic:
      allProducts.value.forEach((p: Product) => {
        stocks.value[p.id!] = Math.floor(Math.random() * 50) + 10;
      });
    } catch (error) {}
  };

  const checkActiveShiftAndLoadRegisters = async (warehouseId: number) => {
    try {
      await store.fetchRegisters();
      if (store.registers.length > 0) {
        selectedRegisterId.value = store.registers[0].id;
      }
      await store.fetchActiveShift();

      if (!store.activeShift) {
        showOpenShiftDialog.value = true;
      }
    } catch (error) {
      console.error("Error loading cash shifts / registers", error);
    }
  };

  const submitOpenShift = async () => {
    if (!selectedRegisterId.value) {
      $q.notify({ color: "warning", message: "Por favor seleccione una caja" });
      return;
    }
    try {
      await store.openShift(
        selectedRegisterId.value,
        openShiftStartingCash.value,
      );
      $q.notify({ color: "positive", message: "Turno abierto correctamente" });
      showOpenShiftDialog.value = false;
    } catch (error: any) {
      $q.notify({ color: "negative", message: "Error al abrir turno" });
    }
  };

  const fetchData = async () => {
    try {
      const [pRes, cRes, wRes, clRes] = await Promise.all([
        productService.getAll(),
        categoryService.getAll(),
        warehouseService.getAll(),
        clientService.getAll(),
      ]);
      allProducts.value = pRes.data;
      categories.value = cRes.data;
      warehouses.value = wRes.data;
      clients.value = clRes.data;

      if (warehouses.value.length > 0) {
        formData.warehouseId =
          warehouseStore.activeWarehouseId || warehouses.value[0].id!;
        fetchStocks();
        await checkActiveShiftAndLoadRegisters(formData.warehouseId);
      }
      if (clients.value.length > 0) {
        formData.clientId = clients.value[0].id!;
      }
    } catch (error) {
      $q.notify({ color: "negative", message: "Error al cargar datos" });
    }
  };

  const incrementQty = (index: number) => {
    const item = cart.value[index];
    updateCartItem(item, item.quantity + 1);
  };

  const decrementQty = (index: number) => {
    const item = cart.value[index];
    if (item.quantity > 1) {
      updateCartItem(item, item.quantity - 1);
    }
  };

  const removeFromCart = (index: number) => {
    cart.value.splice(index, 1);
    calculateTotals();
  };

  const clearCart = () => {
    cart.value = [];
    paidAmount.value = 0;
    formData.taxRate = 0;
    formData.discount = 0;
    formData.shipping = 0;
    formData.notes = "";
  };

  const submitSale = async () => {
    saving.value = true;
    try {
      const sale: Sale = {
        ...formData,
        ref: `POS-${Date.now()}`,
        isPos: true,
        grandTotal: total.value,
        paidAmount: total.value,
        paymentStatus: "paid",
        status: "completed",
        details: cart.value,
      };

      const res = await saleService.create(sale);
      const saleId = res.data?.id || 1;

      clearCart();
      showCheckoutDialog.value = false;

      $q.dialog({
        title: "Venta Exitosa",
        message: `Venta realizada con éxito.\nTotal: ${formatCurrency(total.value)}\nCambio: ${formatCurrency(change.value)}`,
        persistent: true,
        ok: {
          label: "Imprimir Comprobante",
          color: "primary",
          unelevated: true,
        },
        cancel: { label: "Cerrar", flat: true, color: "grey" },
      }).onOk(() => {
        router.push(`/sales/print/${saleId}`);
      });
    } catch (error) {
      $q.notify({ color: "negative", message: "Error al procesar la venta" });
    } finally {
      saving.value = false;
    }
  };

  const openCheckout = () => {
    paidAmount.value = total.value;
    calculateChange();
    showCartMobile.value = false;
    showCheckoutDialog.value = true;
  };

  const searchClientByNit = async (nit: string) => {
    if (!nit || nit === "0") {
      formData.clientId = clients.value.find((c) => c.nitCi === "0")?.id || 0;
      formData.clientNit = "0";
      formData.clientName = "Sin nombre";
      return;
    }

    isSearchingClient.value = true;
    try {
      const res = await clientService.searchByNit(nit);
      if (res.data) {
        formData.clientId = res.data.id!;
        formData.clientNit = res.data.nitCi;
        formData.clientName = res.data.name;
      } else {
        // Not found, will be handled by the UI to open the modal
        throw new Error("Not found");
      }
    } catch (error) {
      throw error; // Let the component handle opening the modal
    } finally {
      isSearchingClient.value = false;
    }
  };

  const quickRegisterClient = async (clientData: Partial<Client>) => {
    try {
      const res = await clientService.create(clientData as Client);
      const newClient = res.data;
      clients.value.push(newClient);
      formData.clientId = newClient.id!;
      formData.clientNit = newClient.nitCi;
      formData.clientName = newClient.name;
      return newClient;
    } catch (error) {
      $q.notify({ color: "negative", message: "Error al registrar cliente" });
      throw error;
    }
  };

  return {
    // State
    search,
    selectedCategory,
    categories,
    allProducts,
    warehouses,
    clients,
    stocks,
    cart,
    showCheckoutDialog,
    showCartMobile,
    showScanner,
    showScanResultDialog,
    lastScannedProduct,
    paidAmount,
    change,
    saving,
    formData,
    quickAmounts,
    isSearchingClient,
    isInvoiceBlocked,
    SIN_LIMIT,
    activeShift,
    registers,
    showOpenShiftDialog,
    openShiftStartingCash,
    selectedRegisterId,
    // Computed
    subtotal,
    total,
    filteredProducts,
    // Methods
    calculateTotals,
    setQuickAmount,
    handleBarcodeScan,
    getProductName,
    getStock,
    fetchData,
    fetchStocks,
    addToCart,
    incrementQty,
    decrementQty,
    removeFromCart,
    clearCart,
    calculateChange,
    submitSale,
    openCheckout,
    searchClientByNit,
    quickRegisterClient,
    submitOpenShift,
    checkActiveShiftAndLoadRegisters,
  };
}
