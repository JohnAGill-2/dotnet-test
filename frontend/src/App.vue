<script setup>
import { computed } from "vue";
import { useRouter } from "vue-router";
import { clearSession, getSession, isAuthenticated } from "./auth";

const router = useRouter();

const authState = computed(() => {
  if (!isAuthenticated()) {
    return null;
  }

  return getSession();
});

function logout() {
  clearSession();
  router.push({ name: "Login" });
}
</script>

<template>
  <div id="app" class="min-h-screen bg-gray-50">
    <!-- Header -->
    <header class="bg-white shadow">
      <nav class="max-w-7xl mx-auto px-4 py-4 sm:px-6 lg:px-8">
        <div class="flex justify-between items-center">
          <router-link to="/" class="text-2xl font-bold text-blue-600 hover:text-blue-800">
            Recommendation Engine
          </router-link>
          <div class="flex gap-4 items-center">
            <template v-if="authState">
              <span class="text-sm text-gray-600">
                {{ authState.userName }} ({{ authState.role }})
              </span>
              <button class="text-sm text-blue-600 hover:text-blue-800" @click="logout">
                Logout
              </button>
            </template>
            <router-link v-else to="/login" class="text-sm text-blue-600 hover:text-blue-800">
              Login
            </router-link>
            <a href="http://localhost:5041/swagger/index.html" target="_blank" class="text-gray-600 hover:text-gray-900 text-sm">
              API Docs
            </a>
          </div>
        </div>
      </nav>
    </header>

    <!-- Main Content -->
    <main class="max-w-7xl mx-auto px-4 py-8 sm:px-6 lg:px-8">
      <router-view />
    </main>

    <!-- Footer -->
    <footer class="bg-white border-t border-gray-200 mt-12">
      <div class="max-w-7xl mx-auto px-4 py-6 sm:px-6 lg:px-8">
        <p class="text-gray-600 text-sm">
          Sportsbook Recommendation Engine - Week 4 Demo
        </p>
      </div>
    </footer>
  </div>
</template>

<style>
body {
  margin: 0;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Roboto', 'Oxygen',
    'Ubuntu', 'Cantarell', 'Fira Sans', 'Droid Sans', 'Helvetica Neue', sans-serif;
}
</style>
