/*global module:false*/
module.exports = function(grunt) {

  // Project configuration.
  grunt.initConfig({
    watch: {
      files: ['*.html'],
      tasks: 'htmllint'
    },
    htmllint: {
        files: '*.html'
    },
    server: {
        port: 3000,
        base: '.'
    }
  });

  grunt.loadNpmTasks('grunt-html');
  grunt.registerTask('default', 'server watch');

};
