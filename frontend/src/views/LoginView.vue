<script setup>
import { ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { login } from "../api";
import { saveSession } from "../auth";

const route = useRoute();
const router = useRouter();

const userName = ref("");
const password = ref("");
const loading = ref(false);
const error = ref("");

async function submit() {
  loading.value = true;
  error.value = "";

  try {
    const result = await login(userName.value, password.value);
    saveSession(result);

    const redirect = typeof route.query.redirect === "string" ? route.query.redirect : "/";
    router.replace(redirect);
  } catch (err) {
    error.value = err?.message || "Login failed.";
  } finally {
    loading.value = false;
  }
}
</script>

<template>
  <section class="login-view">
    <article class="login-card">
      <h1>Sign in</h1>
      <p class="subtitle">Use a demo account to continue.</p>

      <div v-if="error" class="error-alert">{{ error }}</div>

      <form class="form" @submit.prevent="submit">
        <label>
          <span>Username</span>
          <input v-model="userName" type="text" autocomplete="username" required />
        </label>

        <label>
          <span>Password</span>
          <input v-model="password" type="password" autocomplete="current-password" required />
        </label>

        <button class="btn-primary" :disabled="loading" type="submit">
          {{ loading ? "Signing in..." : "Sign in" }}
        </button>
      </form>

      <div class="hint">
        <p><strong>Admin:</strong> admin / admin123!</p>
        <p><strong>User:</strong> user12345 / user12345!</p>
      </div>
    </article>
  </section>
</template>

<style scoped>
.login-view {
  min-height: 60vh;
  display: grid;
  place-items: center;
}

.login-card {
  width: min(460px, 100%);
  border: 1px solid #e5e7eb;
  background: #ffffff;
  border-radius: 14px;
  padding: 18px;
  display: grid;
  gap: 14px;
}

.subtitle {
  margin: 0;
  color: #6b7280;
}

.form {
  display: grid;
  gap: 12px;
}

label {
  display: grid;
  gap: 6px;
}

label span {
  font-size: 13px;
  color: #4b5563;
  font-weight: 600;
}

input {
  border: 1px solid #d1d5db;
  border-radius: 10px;
  padding: 10px 12px;
  font-size: 14px;
}

.hint {
  border-top: 1px dashed #e5e7eb;
  padding-top: 10px;
  color: #4b5563;
  font-size: 13px;
}

.hint p {
  margin: 4px 0;
}
</style>
