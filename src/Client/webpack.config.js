const webpackMerge = require('webpack-merge');
const webpack = require('webpack');
const path = require('path');
const baseWebpackConfig = require('@kentico/xperience-webpack-config');

module.exports = (opts, argv) => {
  const isDevelopment = argv.mode === 'development';
  
  const baseConfig = (webpackConfigEnv, argv) => {
    return baseWebpackConfig({
      orgName: 'xperiencecommunity',
      projectName: 'admin-extensions',
      webpackConfigEnv: webpackConfigEnv,
      argv: argv,
    });
  };

  const projectConfig = {
    mode: isDevelopment ? 'development' : 'production',
    devtool: isDevelopment ? 'source-map' : false,
    optimization: {
      minimize: !isDevelopment,
    },
    output: {
      clean: true, // Clean the output directory before emit
    },
    devServer: {
          port: 3009,
    },
    plugins: [
      new webpack.DefinePlugin({
        'process.env.NODE_ENV': JSON.stringify(isDevelopment ? 'development' : 'production')
      })
    ],
    module: {
      rules: [
        {
          test: /\.(js|ts)x?$/,
          exclude: /node_modules/,
          use: {
            loader: 'babel-loader'
          }
        },
        {
          test: /\.css$/,
          use: ['style-loader', 'css-loader', 'postcss-loader']
        }
      ]
    },
    resolve: {
      extensions: ['.ts', '.tsx', '.js', '.jsx'],
      alias: {
        '@': path.resolve(__dirname, 'src')
      }
    }
  };

  return webpackMerge.merge(projectConfig, baseConfig(opts, argv));
};
