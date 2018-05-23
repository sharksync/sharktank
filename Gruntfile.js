module.exports = function (grunt) {

    grunt.initConfig({
        pkg: grunt.file.readJSON('package.json'),

        cacheBust: {
            indexCacheBust: {
                options: {
                    deleteOriginals: true,
                    assets: ['dist/*.js', 'dist/*.css'],
                    baseDir: 'SharkSync.Web/bin/Release/netcoreapp2.0/publish/wwwroot/'
                },
                src: ['SharkSync.Web/bin/Release/netcoreapp2.0/publish/wwwroot/index.html']
            }
        }

    });

    // Plugins used
    grunt.loadNpmTasks('grunt-cache-bust');

    // Register pre and post build tasks
    grunt.registerTask('postBuild', ['cacheBust:indexCacheBust']);

};